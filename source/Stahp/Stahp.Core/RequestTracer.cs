using HtmlAgilityPack;

using Microsoft.Extensions.Logging;

using Stahp.Core.HostTypes;

using System.Diagnostics.CodeAnalysis;
using System.Net;

using Whois;

namespace Stahp.Core
{
    public class RequestTracer : IRequestTracer
    {
        private readonly HttpClient _httpClient;
        private readonly WhoisLookup _whoisClient;
        private readonly ILogger<RequestTracer> _logger;

        public RequestTracer(IHttpClientFactory httpClientFactory, WhoisLookup whoisClient, ILogger<RequestTracer> logger)
        {
            // the resolved HttpClient when registered as a typed client doesn't seem to obey the primary http client handler configuration
            //  when using constructor injection
            // retrieving via IHttpClientFactory does work though
            _httpClient = httpClientFactory.CreateClient(nameof(RequestTracer));
            _whoisClient = whoisClient;
            _logger = logger;
        }

        public async Task<TraceResult?> TraceUrl([NotNull] Uri url)
        {
            TraceResult? result = null;

            try
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);
                _logger.LogInformation("Starting trace for {url}", url);
                HttpResponseMessage response = await _httpClient.SendAsync(request);

                result = response.StatusCode switch
                {
                    >= HttpStatusCode.MovedPermanently and <= HttpStatusCode.PermanentRedirect => await RedirectResult(response),
                    HttpStatusCode.OK => await OkResult(response),
                    _ => await UnknownResult(response)
                };

                if (result.Redirects)
                {
                    result.NextHop = await TraceUrl(result.RedirectTargetUrl);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Performing trace on {url} encountered an error.", url);
            }

            return result;
        }

        private async Task<TraceResult> OkResult(HttpResponseMessage response)
        {
            // we've either hit the final destination, or
            // the page will redirect via
            //  - a HTML Meta Refresh tag, or
            //  - TODO: setting the JavaScript window.location property

            HtmlDocument htmlDoc = new();
            htmlDoc.Load(await response.Content.ReadAsStreamAsync());

            // see https://developer.mozilla.org/en-US/docs/Web/HTML/Element/meta#attr-http-equiv
            // content attribute format matches "[seconds];url=[address]"
            HtmlNode? refreshTag = htmlDoc.DocumentNode.Descendants("meta")
                .FirstOrDefault(node =>
                    string.Equals(node.GetAttributeValue("http-equiv", string.Empty), "refresh", StringComparison.OrdinalIgnoreCase) &&
                    node.GetAttributeValue("content", string.Empty).Contains("url=", StringComparison.OrdinalIgnoreCase));

            if (refreshTag is not null)
            {
                string redirectTarget = refreshTag.GetAttributeValue("content", string.Empty)
                    .Substring(refreshTag.GetAttributeValue("content", string.Empty)
                    .IndexOf("url=", StringComparison.OrdinalIgnoreCase) + 4);

                return new(response.RequestMessage.RequestUri, response.StatusCode)
                {
                    Redirects = true,
                    RedirectTargetUrl = new Uri(redirectTarget),
                    DomainHost = await DetermineHost(response.RequestMessage.RequestUri),
                };
            }

            return new(response.RequestMessage.RequestUri, response.StatusCode)
            {
                DomainHost = await DetermineHost(response.RequestMessage.RequestUri),
            };
        }

        private async Task<TraceResult> RedirectResult([NotNull] HttpResponseMessage response)
        {
            return new(response.RequestMessage.RequestUri, response.StatusCode)
            {
                Redirects = true,
                RedirectTargetUrl = response.Headers.Location,
                DomainHost = await DetermineHost(response.RequestMessage.RequestUri),
            };
        }

        private async Task<TraceResult> UnknownResult([NotNull] HttpResponseMessage response)
        {
            return new(response.RequestMessage.RequestUri, response.StatusCode)
            {
                DomainHost = await DetermineHost(response.RequestMessage.RequestUri),
            };
        }

        private async Task<IHost> DetermineHost([NotNull] Uri uri)
        {
            // TODO: this is lacking. use a command pattern in a proper factory?
            // Regex match on known formats for AWS and Azure?
            if (uri.Host.EndsWith(".amazonaws.com", StringComparison.OrdinalIgnoreCase))
            {
                return new AmazonS3BucketHost(uri);
            }
            if (uri.Host.EndsWith(".blob.core.windows.net", StringComparison.OrdinalIgnoreCase))
            {
                return new AzureStorageBlobHost(uri);
            }

            // do a whois lookup to determine host and abuse contact details
            WhoisResponse? whoisResponse = await WhoisLookup(uri.Host);

            return new NonspecificHost
            {
                HostName = whoisResponse.Registrar?.Name,
                HostUrl = whoisResponse.Registrar?.Url,
                AbuseContact = whoisResponse.Registrar?.AbuseEmail,
            };
        }

        private async Task<WhoisResponse?> WhoisLookup(string domainName)
        {
            try
            {
                WhoisResponse? response = await _whoisClient.LookupAsync(domainName);

                if (response?.Registrar is null)
                {
                    response = await WhoisLookup(domainName[(domainName.IndexOf('.') + 1)..]);
                }
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Encountered an error performing whois query on {domain}", domainName);
            }
            return null;
        }
    }
}