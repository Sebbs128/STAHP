using AngleSharp;
using AngleSharp.Dom;

using Microsoft.Extensions.Caching.Memory;

using Stahp.Core.HostTypes;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stahp.Core.HttpResponseProcessing
{
    internal class JsRedirectProcessor : HttpResponseProcessorBase, IHttpResponseProcessor
    {
        private readonly IMemoryCache _memoryCache;
        private readonly IConfiguration _anglesharpConfig;
        private static readonly MemoryCacheEntryOptions _cacheEntryOptions = new()
        {
            SlidingExpiration = TimeSpan.FromMinutes(10)
        };

        public JsRedirectProcessor(IMemoryCache memoryCache, IConfiguration anglesharpConfig, IHostFactory hostFactory) : base(hostFactory)
        {
            _memoryCache = memoryCache;
            _anglesharpConfig = anglesharpConfig;
        }

        private static string GetCacheKey(HttpResponseMessage responseMessage) => $"{nameof(JsRedirectProcessor)}_{responseMessage.RequestMessage!.RequestUri}";

        public override async Task<bool> CanProcess(HttpResponseMessage httpResponseMessage)
        {
            if (httpResponseMessage.StatusCode != System.Net.HttpStatusCode.OK)
                return false;

            IDocument htmlDoc = await BrowsingContext.New(_anglesharpConfig)
                .OpenAsync(async req => req.Content(await httpResponseMessage.Content.ReadAsStreamAsync()));

            Uri currentUri = new(htmlDoc.DocumentUri);

            if (!currentUri.Host.Equals(httpResponseMessage.RequestMessage!.RequestUri!.Host))
            {
                _memoryCache.Set(GetCacheKey(httpResponseMessage), currentUri, _cacheEntryOptions);
                return true;
            }
            return false;
        }

        public override async Task<TraceHop> Process(HttpResponseMessage httpResponseMessage)
        {
            Uri newUri = _memoryCache.Get<Uri>(GetCacheKey(httpResponseMessage))!;

            return new TraceHop(httpResponseMessage.RequestMessage!.RequestUri!, httpResponseMessage.StatusCode)
            {
                RedirectType = RedirectType.JsHref,
                RedirectTargetUrl = newUri,
                DomainHost = await DetermineHost(httpResponseMessage.RequestMessage!.RequestUri!),
                WebHost = await DetermineWebHost(httpResponseMessage.RequestMessage!.RequestUri!),
            };
        }
    }
}
