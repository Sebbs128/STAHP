using Stahp.Core.HostTypes;

namespace Stahp.Core.HttpResponseProcessing
{
    internal class DefaultHttpResponseProcessor : HttpResponseProcessorBase, IHttpResponseProcessor
    {
        public DefaultHttpResponseProcessor(IHostFactory hostFactory) : base(hostFactory)
        {
        }

        public override async Task<TraceHop?> Process(HttpResponseMessage httpResponseMessage)
        {
            return new TraceHop(httpResponseMessage.RequestMessage!.RequestUri!, httpResponseMessage.StatusCode)
            {
                DomainHost = await DetermineHost(httpResponseMessage.RequestMessage.RequestUri!),
                WebHost = await DetermineWebHost(httpResponseMessage.RequestMessage.RequestUri!),
            };
        }
    }
}
