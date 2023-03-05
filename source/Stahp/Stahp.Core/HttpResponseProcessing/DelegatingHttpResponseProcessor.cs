using Stahp.Core.HostTypes;

namespace Stahp.Core.HttpResponseProcessing
{
    internal abstract class DelegatingHttpResponseProcessor : HttpResponseProcessorBase, IHttpResponseProcessor
    {
        public IHttpResponseProcessor? InnerProcessor { get; set; }

        protected DelegatingHttpResponseProcessor(IHostFactory hostFactory) : base(hostFactory)
        {
        }

        public override async Task<TraceHop?> Process(HttpResponseMessage httpResponseMessage)
        {
            return InnerProcessor is not null 
                ? await InnerProcessor.Process(httpResponseMessage)
                : null;
        }
    }
}
