using AngleSharp;
using AngleSharp.Dom;

using Microsoft.Extensions.Caching.Memory;
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
    internal class HtmlRedirectProcessor : HttpResponseProcessorBase, IHttpResponseProcessor
    {
        private readonly IMemoryCache _memoryCache;
        private readonly IConfiguration _anglesharpConfig;
        private static readonly MemoryCacheEntryOptions _cacheEntryOptions = new()
        {
            SlidingExpiration = TimeSpan.FromMinutes(10)
        };

        public HtmlRedirectProcessor(IMemoryCache memoryCache, IConfiguration anglesharpConfig, IHostFactory hostFactory) : base(hostFactory)
        {
            _memoryCache = memoryCache;
            _anglesharpConfig = anglesharpConfig;
        }

        private static string GetCacheKey(HttpResponseMessage responseMessage) => $"{nameof(HtmlRedirectProcessor)}_{responseMessage.RequestMessage!.RequestUri}";

        public override async Task<bool> CanProcess(HttpResponseMessage httpResponseMessage)
        {
            if (httpResponseMessage.StatusCode != HttpStatusCode.OK)
                return false;

            // we've either hit the final destination, or
            // the page will redirect via
            //  - a HTML Meta Refresh tag (this IHttpResponseProcessor), or
            //  - setting the JavaScript window.location property (JsRedirectProcessor)
            IDocument htmlDoc = await BrowsingContext.New(_anglesharpConfig).OpenAsync(async req => 
                req.Content(await httpResponseMessage.Content.ReadAsStreamAsync()));

            // see https://developer.mozilla.org/en-US/docs/Web/HTML/Element/meta#attr-http-equiv
            // content attribute format matches "[seconds]; url=[address]" (space between ";" and "url=" is optional
            IElement? refreshTag = htmlDoc.Head
                ?.QuerySelectorAll("meta")
                ?.FirstOrDefault(node =>
                    string.Equals(node.GetAttribute("http-equiv"), "refresh", StringComparison.OrdinalIgnoreCase) &&
                    node.GetAttribute("content")?.Contains("url=", StringComparison.OrdinalIgnoreCase) == false);

            if (refreshTag is not null)
            {
                _memoryCache.Set(GetCacheKey(httpResponseMessage), refreshTag, _cacheEntryOptions);
            }
            return refreshTag is not null;
        }

        public override async Task<TraceHop> Process(HttpResponseMessage httpResponseMessage)
        {
            IElement refreshTag = _memoryCache.Get<IElement>(GetCacheKey(httpResponseMessage))!;

            string refreshTagContent = refreshTag.GetAttribute("content")!;

            string redirectTarget = refreshTagContent![(refreshTagContent.IndexOf("url=", StringComparison.OrdinalIgnoreCase) + 4)..];

            return new TraceHop(httpResponseMessage.RequestMessage!.RequestUri!, httpResponseMessage.StatusCode)
            {
                RedirectType = RedirectType.HtmlMeta,
                RedirectTargetUrl = new Uri(redirectTarget),
                DomainHost = await DetermineHost(httpResponseMessage.RequestMessage!.RequestUri!),
                WebHost = await DetermineWebHost(httpResponseMessage.RequestMessage!.RequestUri!),
            };
        }
    }
}
