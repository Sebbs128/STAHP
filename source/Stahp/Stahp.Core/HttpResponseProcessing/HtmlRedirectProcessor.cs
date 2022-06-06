using HtmlAgilityPack;

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
        private static readonly MemoryCacheEntryOptions _cacheEntryOptions = new()
        {
            SlidingExpiration = TimeSpan.FromMinutes(10)
        };

        public HtmlRedirectProcessor(IMemoryCache memoryCache, IHostFactory hostFactory) : base(hostFactory)
        {
            _memoryCache = memoryCache;
        }

        private static string GetCacheKey(HttpResponseMessage responseMessage) => $"{nameof(HtmlRedirectProcessor)}_{responseMessage.RequestMessage!.RequestUri}";

        public override async Task<bool> CanProcess(HttpResponseMessage httpResponseMessage)
        {
            if (httpResponseMessage.StatusCode != HttpStatusCode.OK)
                return false;

            // we've either hit the final destination, or
            // the page will redirect via
            //  - a HTML Meta Refresh tag, or
            //  - setting the JavaScript window.location property
            //    TODO: create new IHttpResponseProcessor implementation (JSRedirectProcessor) to handle JavaScript check
            HtmlDocument htmlDoc = new();
            htmlDoc.Load(await httpResponseMessage.Content.ReadAsStreamAsync());

            // see https://developer.mozilla.org/en-US/docs/Web/HTML/Element/meta#attr-http-equiv
            // content attribute format matches "[seconds]; url=[address]" (space between ";" and "url=" is optional
            HtmlNode? refreshTag = htmlDoc.DocumentNode.Descendants("meta")
                .FirstOrDefault(node =>
                    string.Equals(node.GetAttributeValue("http-equiv", string.Empty), "refresh", StringComparison.OrdinalIgnoreCase) &&
                    node.GetAttributeValue("content", string.Empty).Contains("url=", StringComparison.OrdinalIgnoreCase));

            if (refreshTag is not null)
            {
                _memoryCache.Set(GetCacheKey(httpResponseMessage), refreshTag, _cacheEntryOptions);
            }
            return refreshTag is not null;
        }

        public override async Task<TraceHop> Process(HttpResponseMessage httpResponseMessage)
        {
            HtmlNode refreshTag = _memoryCache.Get<HtmlNode>(GetCacheKey(httpResponseMessage));

            string redirectTarget = refreshTag.GetAttributeValue("content", string.Empty)
                    .Substring(refreshTag.GetAttributeValue("content", string.Empty)
                    .IndexOf("url=", StringComparison.OrdinalIgnoreCase) + 4);

            return new TraceHop(httpResponseMessage.RequestMessage!.RequestUri!, httpResponseMessage.StatusCode)
            {
                Redirects = true,
                RedirectTargetUrl = new Uri(redirectTarget),
                DomainHost = await DetermineHost(httpResponseMessage.RequestMessage!.RequestUri!),
                WebHost = await DetermineWebHost(httpResponseMessage.RequestMessage!.RequestUri!),
            };
        }
    }
}
