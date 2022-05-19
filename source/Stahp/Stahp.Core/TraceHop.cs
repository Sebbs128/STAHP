using Stahp.Core.HostTypes;

using System.Net;

namespace Stahp.Core
{
    public class TraceHop
    {
        public Uri Url { get; set; }
        public HttpStatusCode HttpStatusCode { get; set; }
        public bool Redirects { get; set; }

        public Uri? RedirectTargetUrl { get; set; }

        // Who the Domain is registered through
        public IHost? DomainHost { get; set; }

        // Who the website is hosted by
        public IHost? WebHost { get; set; }

        public TraceHop? NextHop { get; set; }

        public TraceHop(Uri url, HttpStatusCode httpStatusCode)
        {
            Url = url;
            HttpStatusCode = httpStatusCode;
        }
    }
}
