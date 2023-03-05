using Stahp.Core.HostTypes;

using System.Net;

namespace Stahp.Core.HttpResponseProcessing
{
    internal class HttpRedirectProcessor : DelegatingHttpResponseProcessor, IHttpResponseProcessor
    {
        public HttpRedirectProcessor(IHostFactory hostFactory) : base(hostFactory)
        {
        }

        public override async Task<TraceHop?> Process(HttpResponseMessage httpResponseMessage)
        {
            return httpResponseMessage.StatusCode switch
            {
                >= HttpStatusCode.MovedPermanently and <= HttpStatusCode.PermanentRedirect => new TraceHop(httpResponseMessage.RequestMessage!.RequestUri!, httpResponseMessage.StatusCode)
                {
                    RedirectTargetUrl = httpResponseMessage.Headers.Location,
                    DomainHost = await DetermineHost(httpResponseMessage.RequestMessage.RequestUri!),
                    WebHost = await DetermineWebHost(httpResponseMessage.RequestMessage.RequestUri!),
                },
                _ => await base.Process(httpResponseMessage)
            }; ;
        }
    }
}
