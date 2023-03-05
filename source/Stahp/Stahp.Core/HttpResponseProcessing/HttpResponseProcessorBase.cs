using Microsoft.Extensions.Logging;

using Stahp.Core.HostTypes;

using System.Net;

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

        public abstract Task<TraceHop?> Process(HttpResponseMessage httpResponseMessage);

        protected async Task<IHost> DetermineHost(Uri requestUri)
        {
            return await _hostFactory.GetHost(requestUri);
        }

        protected async Task<IHost?> DetermineWebHost(Uri requestUri)
        {
            IPHostEntry? dnsHostEntry = await Dns.GetHostEntryAsync(requestUri!.Host);

            return dnsHostEntry is not null
                    ? await DetermineHost(new Uri($"http://{dnsHostEntry.HostName}"))
                    : null;
        }
    }
}