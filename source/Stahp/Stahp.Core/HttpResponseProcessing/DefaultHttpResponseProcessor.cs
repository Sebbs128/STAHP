using Microsoft.Extensions.Logging;

using Stahp.Core.HostTypes;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Whois;

namespace Stahp.Core.HttpResponseProcessing
{
    internal class DefaultHttpResponseProcessor : HttpResponseProcessorBase, IHttpResponseProcessor
    {
        public DefaultHttpResponseProcessor(IHostFactory hostFactory) : base(hostFactory)
        {
        }

        public override Task<bool> CanProcess(HttpResponseMessage httpResponseMessage)
        {
            return Task.FromResult(true);
        }

        public override async Task<TraceHop> Process(HttpResponseMessage httpResponseMessage)
        {
            return new TraceHop(httpResponseMessage.RequestMessage!.RequestUri!, httpResponseMessage.StatusCode)
            {
                DomainHost = await DetermineHost(httpResponseMessage.RequestMessage.RequestUri!),
                WebHost = await DetermineWebHost(httpResponseMessage.RequestMessage.RequestUri!),
            };
        }
    }
}
