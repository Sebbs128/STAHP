using Stahp.Core.HostTypes;

using System.Net;

namespace Stahp.Core
{
    public class TraceHop
    {
        public Uri Url { get; set; }
        public HttpStatusCode? HttpStatusCode { get; set; }
        public bool Redirects { get; set; }

        public string? ErrorMessage { get; set; }

        public Uri? RedirectTargetUrl { get; set; }

        // TODO: consider combining DomainHost and WebHost into a Dictionary<HostType, IHost>
        // Who the Domain is registered through
        public IHost? DomainHost { get; set; }

        // Who the website is hosted by
        public IHost? WebHost { get; set; }

        public TraceHop? NextHop { get; set; }

        public TraceHop(Uri url)
        {
            Url = url;
        }

        public TraceHop(Uri url, HttpStatusCode httpStatusCode) : this(url)
        {
            HttpStatusCode = httpStatusCode;
        }

        public TraceHop(Uri url, Exception exception) : this(url)
        {
            ErrorMessage = exception.Message;
        }
    }
}
