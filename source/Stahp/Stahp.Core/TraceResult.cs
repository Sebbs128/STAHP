using Stahp.Core.HostTypes;

using System.Net;

namespace Stahp.Core
{
    public class TraceResult
    {
        public Uri Url { get; set; }
        public HttpStatusCode HttpStatusCode { get; set; }
        public bool Redirects { get; set; }

        public Uri? RedirectTargetUrl { get; set; }

        // Who the Domain is registered through
        public IHost? DomainHost { get; set; }

        // Who the website is hosted by
        public IHost? WebHost { get; set; }

        public TraceResult? NextHop { get; set; }

        public TraceResult(Uri url, HttpStatusCode httpStatusCode)
        {
            Url = url;
            HttpStatusCode = httpStatusCode;
        }
    }
}
