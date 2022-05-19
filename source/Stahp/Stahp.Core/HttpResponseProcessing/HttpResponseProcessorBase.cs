using Microsoft.Extensions.Logging;

using Stahp.Core.HostTypes;

using Whois;

namespace Stahp.Core.HttpResponseProcessing
{
    internal abstract class HttpResponseProcessorBase
    {
        private readonly IHostFactory _hostFactory;

        public HttpResponseProcessorBase(IHostFactory hostFactory)
        {
            _hostFactory = hostFactory;
        }

        public abstract Task<bool> CanProcess(HttpResponseMessage httpResponseMessage);
        public abstract Task<TraceHop> Process(HttpResponseMessage httpResponseMessage);

        protected async Task<IHost> DetermineHost(Uri requestUri)
        {
            return await _hostFactory.GetHost(requestUri);
        }
    }
}