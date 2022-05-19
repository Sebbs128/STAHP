
using Microsoft.Extensions.Logging;

using Stahp.Core.HttpResponseProcessing;

namespace Stahp.Core
{
    public class RequestTracer : IRequestTracer
    {
        private readonly HttpClient _httpClient;
        private readonly IEnumerable<IHttpResponseProcessor> _httpResponseProcessors;
        private readonly ILogger<RequestTracer> _logger;

        public RequestTracer(IEnumerable<IHttpResponseProcessor> httpResponseProcessors, IHttpClientFactory httpClientFactory, ILogger<RequestTracer> logger)
        {
            // the resolved HttpClient when registered as a typed client doesn't seem to obey the primary http client handler configuration
            //  when using constructor injection
            // retrieving via IHttpClientFactory does work though
            _httpClient = httpClientFactory.CreateClient(nameof(RequestTracer));
            _httpResponseProcessors = httpResponseProcessors;
            _logger = logger;
        }

        public async IAsyncEnumerable<TraceHop> TraceUrlAsync(Uri url)
        {
            bool reachedEnd = false;
            Uri nextUrl = url;
            do
            {
                TraceHop? hop = await GetNextHop(nextUrl);

                if (hop is not null)
                {
                    yield return hop;

                    if (hop.Redirects && hop.RedirectTargetUrl is not null)
                    {
                        nextUrl = hop.RedirectTargetUrl;
                    }
                    else
                    {
                        reachedEnd = true;
                    }
                }
                else
                    reachedEnd = true;
            } while (!reachedEnd);
        }

        private async Task<TraceHop?> GetNextHop(Uri url)
        {
            try
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);
                _logger.LogInformation("Starting trace for {url}", url);
                HttpResponseMessage response = await _httpClient.SendAsync(request);

                foreach (var processor in _httpResponseProcessors)
                {
                    if (await processor.CanProcess(response))
                    {
                        return await processor.Process(response);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Performing trace on {url} encountered an error.", url);
            }

            return null;
        }
    }
}