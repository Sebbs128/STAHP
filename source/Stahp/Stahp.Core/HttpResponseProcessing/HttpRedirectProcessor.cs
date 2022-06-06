using Microsoft.Extensions.Logging;

using Stahp.Core.HostTypes;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

using Whois;

namespace Stahp.Core.HttpResponseProcessing
{
    internal class HttpRedirectProcessor : HttpResponseProcessorBase, IHttpResponseProcessor
    {
        public HttpRedirectProcessor(IHostFactory hostFactory) : base(hostFactory)
        {
        }

        public override Task<bool> CanProcess(HttpResponseMessage httpResponseMessage)
        {
            return Task.FromResult(httpResponseMessage.StatusCode >= HttpStatusCode.MovedPermanently 
                && httpResponseMessage.StatusCode <= HttpStatusCode.PermanentRedirect);
        }

        public override async Task<TraceHop> Process(HttpResponseMessage httpResponseMessage)
        {
            return new TraceHop(httpResponseMessage.RequestMessage!.RequestUri!, httpResponseMessage.StatusCode)
            {
                Redirects = true,
                RedirectTargetUrl = httpResponseMessage.Headers.Location,
                DomainHost = await DetermineHost(httpResponseMessage.RequestMessage.RequestUri),
                WebHost = await DetermineWebHost(httpResponseMessage.RequestMessage.RequestUri),
            };
        }
    }
}
