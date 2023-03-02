
using Microsoft.Extensions.Logging;

using Stahp.Core.ExceptionProcessing;
using Stahp.Core.HttpResponseProcessing;

using System.Net.Sockets;

namespace Stahp.Core
{
    public class RequestTracer : IRequestTracer
    {
        private readonly HttpClient _httpClient;
        private readonly IEnumerable<IHttpResponseProcessor> _httpResponseProcessors;
        private readonly IEnumerable<IExceptionProcessor> _exceptionProcessors;
        private readonly ILogger<RequestTracer> _logger;

        public RequestTracer(IEnumerable<IHttpResponseProcessor> httpResponseProcessors, IEnumerable<IExceptionProcessor> exceptionProcessors, IHttpClientFactory httpClientFactory, ILogger<RequestTracer> logger)
        {
            // the resolved HttpClient when registered as a typed client doesn't seem to obey the primary http client handler configuration
            //  when using constructor injection
            // retrieving via IHttpClientFactory does work though
            _httpClient = httpClientFactory.CreateClient(nameof(RequestTracer));
            _httpResponseProcessors = httpResponseProcessors;
            _exceptionProcessors = exceptionProcessors;
            _logger = logger;
        }

        public async IAsyncEnumerable<TraceHop> TraceUrlAsync(Uri url)
        {
            bool reachedEnd = false;
            Uri nextUrl = url;
            do
            {
                TraceHop hop = await GetNextHop(nextUrl);

                yield return hop;

                if (hop.Redirects && hop.RedirectTargetUrl is not null)
                {
                    nextUrl = hop.RedirectTargetUrl;
                }
                else
                {
                    reachedEnd = true;
                }
            } while (!reachedEnd);
        }

        private async Task<TraceHop> GetNextHop(Uri url)
        {
            HttpRequestMessage request = new(HttpMethod.Get, url);
            try
            {
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
                foreach (var processor in _exceptionProcessors)
                {
                    if (await processor.CanProcess(ex, request))
                    {
                        return await processor.Process(ex, request);
                    }
                }
            }

            return new TraceHop(url)
            {
                ErrorMessage = "HTTP request could not complete."
            };
        }
    }
}