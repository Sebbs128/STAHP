using AngleSharp;
using AngleSharp.Dom;

using Stahp.Core.HostTypes;

namespace Stahp.Core.HttpResponseProcessing
{
    internal class JsRedirectProcessor : DelegatingHttpResponseProcessor, IHttpResponseProcessor
    {
        private readonly IConfiguration _anglesharpConfig;

        public JsRedirectProcessor(IConfiguration anglesharpConfig, IHostFactory hostFactory) : base(hostFactory)
        {
            _anglesharpConfig = anglesharpConfig;
        }

        public override async Task<TraceHop?> Process(HttpResponseMessage httpResponseMessage)
        {
            if (httpResponseMessage.StatusCode == System.Net.HttpStatusCode.OK)
            {
                using IBrowsingContext browsingContext = BrowsingContext.New(_anglesharpConfig);

                using IDocument htmlDoc = await browsingContext
                    .OpenAsync(async req => req.Content(await httpResponseMessage.Content.ReadAsStreamAsync()))
                    .WhenStable();

                if (!new Uri(htmlDoc.DocumentUri).Host.Equals(httpResponseMessage.RequestMessage!.RequestUri!.Host))
                    return new TraceHop(httpResponseMessage.RequestMessage!.RequestUri!, httpResponseMessage.StatusCode)
                    {
                        RedirectType = RedirectType.JsHref,
                        RedirectTargetUrl = new Uri(htmlDoc.DocumentUri),
                        DomainHost = await DetermineHost(httpResponseMessage.RequestMessage!.RequestUri!),
                        WebHost = await DetermineWebHost(httpResponseMessage.RequestMessage!.RequestUri!),
                    };
            }

            return await base.Process(httpResponseMessage);
        }
    }
}
