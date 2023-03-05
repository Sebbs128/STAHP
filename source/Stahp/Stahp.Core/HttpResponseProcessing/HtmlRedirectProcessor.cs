using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;

using Stahp.Core.HostTypes;

using System.Net;

namespace Stahp.Core.HttpResponseProcessing
{
    internal class HtmlRedirectProcessor : DelegatingHttpResponseProcessor, IHttpResponseProcessor
    {
        private readonly IConfiguration _anglesharpConfig;

        public HtmlRedirectProcessor(IConfiguration anglesharpConfig, IHostFactory hostFactory) : base(hostFactory)
        {
            _anglesharpConfig = anglesharpConfig;
        }

        private async Task<IHtmlMetaElement?> GetRefreshTagFromHttpResponse(HttpResponseMessage httpResponseMessage)
        {
            IDocument htmlDoc = await BrowsingContext.New(_anglesharpConfig).OpenAsync(async req =>
                req.Content(await httpResponseMessage.Content.ReadAsStreamAsync()));

            // see https://developer.mozilla.org/en-US/docs/Web/HTML/Element/meta#attr-http-equiv
            // content attribute format matches "[seconds]; url=[address]" (space between ";" and "url=" is optional
            IHtmlMetaElement? refreshTag = htmlDoc.Head
                ?.GetNodes<IHtmlMetaElement>()
                ?.FirstOrDefault(node =>
                    string.Equals(node.HttpEquivalent, "refresh", StringComparison.OrdinalIgnoreCase) &&
                    node.Content?.Contains("url=", StringComparison.OrdinalIgnoreCase) == true);
            return refreshTag;
        }

        public override async Task<TraceHop?> Process(HttpResponseMessage httpResponseMessage)
        {
            if (httpResponseMessage.StatusCode == HttpStatusCode.OK)
            {
                IHtmlMetaElement? refreshTag = await GetRefreshTagFromHttpResponse(httpResponseMessage);

                if (refreshTag is not null)
                {
                    string redirectTarget = refreshTag.Content![(refreshTag.Content!.IndexOf("url=", StringComparison.OrdinalIgnoreCase) + 4)..];

                    return new TraceHop(httpResponseMessage.RequestMessage!.RequestUri!, httpResponseMessage.StatusCode)
                    {
                        RedirectType = RedirectType.HtmlMeta,
                        RedirectTargetUrl = new Uri(redirectTarget),
                        DomainHost = await DetermineHost(httpResponseMessage.RequestMessage!.RequestUri!),
                        WebHost = await DetermineWebHost(httpResponseMessage.RequestMessage!.RequestUri!),
                    };
                }
            }

            return await base.Process(httpResponseMessage);
        }
    }
}
