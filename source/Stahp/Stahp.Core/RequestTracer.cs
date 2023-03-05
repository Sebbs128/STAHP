
using Microsoft.Extensions.Logging;

using Stahp.Core.ExceptionProcessing;
using Stahp.Core.HttpResponseProcessing.Pipeline;

namespace Stahp.Core
{
    internal class RequestTracer : IRequestTracer
    {
        private readonly HttpClient _httpClient;
        private readonly HttpResponseProcessorPipelineFactory _pipelineFactory;
        private readonly IEnumerable<IExceptionProcessor> _exceptionProcessors;
        private readonly ILogger<RequestTracer> _logger;

        public RequestTracer(HttpResponseProcessorPipelineFactory pipelineFactory, IEnumerable<IExceptionProcessor> exceptionProcessors, HttpClient httpClient, ILogger<RequestTracer> logger)
        {
            _pipelineFactory = pipelineFactory;
            _exceptionProcessors = exceptionProcessors;
            _httpClient = httpClient;
            _logger = logger;
        }

        public async IAsyncEnumerable<TraceHop> TraceUrlAsync(Uri url)
        {
            bool reachedEnd = false;
            Uri nextUrl = url;
            do
            {
                TraceHop? hop = await GetNextHop(nextUrl);

                if (hop is null)
                {
                    reachedEnd = true;
                    continue;
                }

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

        private async Task<TraceHop?> GetNextHop(Uri url)
        {
            HttpRequestMessage request = new(HttpMethod.Get, url);
            try
            {
                _logger.LogInformation("Starting trace for {url}", url);
                HttpResponseMessage response = await _httpClient.SendAsync(request);

                var initialProcessor = _pipelineFactory.Create();
                return await initialProcessor.Process(response);
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