using Stahp.Core.HostTypes;
using Stahp.Core.HttpResponseProcessing;

using System.Net;

namespace Stahp.Core.Tests
{
    [TestClass]
    public class HttpRedirectProcessorTests
    {
        private readonly HttpRedirectProcessor _processor;

        const string RedirectUrl = "https://www.mozilla.org";

        public HttpRedirectProcessorTests()
        {
            var hostFactoryMock = Mocks.HostFactory;

            _processor = new HttpRedirectProcessor(hostFactoryMock);
        }

        [TestMethod]
        [DataRow(HttpStatusCode.Continue, DisplayName = "Status Continue")]
        [DataRow(HttpStatusCode.SwitchingProtocols, DisplayName = "Status SwitchingProtocols")]
        [DataRow(HttpStatusCode.Processing, DisplayName = "Status Processing")]
        [DataRow(HttpStatusCode.EarlyHints, DisplayName = "Status EarlyHints")]
        [DataRow(HttpStatusCode.OK, DisplayName = "Status OK")]
        [DataRow(HttpStatusCode.Created, DisplayName = "Status Created")]
        [DataRow(HttpStatusCode.Accepted, DisplayName = "Status Accepted")]
        [DataRow(HttpStatusCode.NonAuthoritativeInformation, DisplayName = "Status NonAuthoritativeInformation")]
        [DataRow(HttpStatusCode.NoContent, DisplayName = "Status NoContent")]
        [DataRow(HttpStatusCode.ResetContent, DisplayName = "Status ResetContent")]
        [DataRow(HttpStatusCode.PartialContent, DisplayName = "Status PartialContent")]
        [DataRow(HttpStatusCode.MultiStatus, DisplayName = "Status MultiStatus")]
        [DataRow(HttpStatusCode.AlreadyReported, DisplayName = "Status AlreadyReported")]
        [DataRow(HttpStatusCode.IMUsed, DisplayName = "Status IMUsed")]
        [DataRow(HttpStatusCode.Ambiguous, DisplayName = "Status Ambiguous")]
        [DataRow(HttpStatusCode.Unused, DisplayName = "Status Unused")]
        [DataRow(HttpStatusCode.BadRequest, DisplayName = "Status BadRequest")]
        [DataRow(HttpStatusCode.Unauthorized, DisplayName = "Status Unauthorized")]
        [DataRow(HttpStatusCode.PaymentRequired, DisplayName = "Status PaymentRequired")]
        [DataRow(HttpStatusCode.Forbidden, DisplayName = "Status Forbidden")]
        [DataRow(HttpStatusCode.NotFound, DisplayName = "Status NotFound")]
        [DataRow(HttpStatusCode.MethodNotAllowed, DisplayName = "Status MethodNotAllowed")]
        [DataRow(HttpStatusCode.NotAcceptable, DisplayName = "Status NotAcceptable")]
        [DataRow(HttpStatusCode.ProxyAuthenticationRequired, DisplayName = "Status ProxyAuthenticationRequired")]
        [DataRow(HttpStatusCode.RequestTimeout, DisplayName = "Status RequestTimeout")]
        [DataRow(HttpStatusCode.Conflict, DisplayName = "Status Conflict")]
        [DataRow(HttpStatusCode.Gone, DisplayName = "Status Gone")]
        [DataRow(HttpStatusCode.LengthRequired, DisplayName = "Status LengthRequired")]
        [DataRow(HttpStatusCode.PreconditionFailed, DisplayName = "Status PreconditionFailed")]
        [DataRow(HttpStatusCode.RequestEntityTooLarge, DisplayName = "Status RequestEntityTooLarge")]
        [DataRow(HttpStatusCode.RequestUriTooLong, DisplayName = "Status RequestUriTooLong")]
        [DataRow(HttpStatusCode.UnsupportedMediaType, DisplayName = "Status UnsupportedMediaType")]
        [DataRow(HttpStatusCode.RequestedRangeNotSatisfiable, DisplayName = "Status RequestedRangeNotSatisfiable")]
        [DataRow(HttpStatusCode.ExpectationFailed, DisplayName = "Status ExpectationFailed")]
        [DataRow(HttpStatusCode.MisdirectedRequest, DisplayName = "Status MisdirectedRequest")]
        [DataRow(HttpStatusCode.UnprocessableEntity, DisplayName = "Status UnprocessableEntity")]
        [DataRow(HttpStatusCode.Locked, DisplayName = "Status Locked")]
        [DataRow(HttpStatusCode.FailedDependency, DisplayName = "Status FailedDependency")]
        [DataRow(HttpStatusCode.UpgradeRequired, DisplayName = "Status UpgradeRequired")]
        [DataRow(HttpStatusCode.PreconditionRequired, DisplayName = "Status PreconditionRequired")]
        [DataRow(HttpStatusCode.TooManyRequests, DisplayName = "Status TooManyRequests")]
        [DataRow(HttpStatusCode.RequestHeaderFieldsTooLarge, DisplayName = "Status RequestHeaderFieldsTooLarge")]
        [DataRow(HttpStatusCode.UnavailableForLegalReasons, DisplayName = "Status UnavailableForLegalReasons")]
        [DataRow(HttpStatusCode.InternalServerError, DisplayName = "Status InternalServerError")]
        [DataRow(HttpStatusCode.NotImplemented, DisplayName = "Status NotImplemented")]
        [DataRow(HttpStatusCode.BadGateway, DisplayName = "Status BadGateway")]
        [DataRow(HttpStatusCode.ServiceUnavailable, DisplayName = "Status ServiceUnavailable")]
        [DataRow(HttpStatusCode.GatewayTimeout, DisplayName = "Status GatewayTimeout")]
        [DataRow(HttpStatusCode.HttpVersionNotSupported, DisplayName = "Status HttpVersionNotSupported")]
        [DataRow(HttpStatusCode.VariantAlsoNegotiates, DisplayName = "Status VariantAlsoNegotiates")]
        [DataRow(HttpStatusCode.InsufficientStorage, DisplayName = "Status InsufficientStorage")]
        [DataRow(HttpStatusCode.LoopDetected, DisplayName = "Status LoopDetected")]
        [DataRow(HttpStatusCode.NotExtended, DisplayName = "Status NotExtended")]
        [DataRow(HttpStatusCode.NetworkAuthenticationRequired, DisplayName = "Status NetworkAuthenticationRequired")]
        public async Task Process_NotRedirectingStatusCode_ReturnsNull(HttpStatusCode httpStatusCode)
        {
            HttpResponseMessage responseMessage = new HttpResponseMessage(httpStatusCode)
            {
                RequestMessage = new HttpRequestMessage(HttpMethod.Get, "https://localhost"),
            };

            var result = await _processor.Process(responseMessage);

            Assert.IsNull(result);
        }

        [TestMethod]
        [DataRow(HttpStatusCode.MovedPermanently, RedirectUrl, DisplayName = "Status MovedPermanently")]
        [DataRow(HttpStatusCode.Found, RedirectUrl, DisplayName = "Status Found")]
        [DataRow(HttpStatusCode.Redirect, RedirectUrl, DisplayName = "Status Redirect")]
        [DataRow(HttpStatusCode.RedirectMethod, RedirectUrl, DisplayName = "Status RedirectMethod")]
        [DataRow(HttpStatusCode.NotModified, RedirectUrl, DisplayName = "Status NotModified")]
        [DataRow(HttpStatusCode.UseProxy, RedirectUrl, DisplayName = "Status UseProxy")]
        [DataRow(HttpStatusCode.TemporaryRedirect, RedirectUrl, DisplayName = "Status TemporaryRedirect")]
        [DataRow(HttpStatusCode.PermanentRedirect, RedirectUrl, DisplayName = "Status PermanentRedirect")]
        public async Task Process_RedirectingStatusCode_ReturnsRedirectToUrl(HttpStatusCode httpStatusCode, string redirectTargetUrl)
        {
            Uri redirectTarget = new Uri(redirectTargetUrl);
            HttpResponseMessage responseMessage = new HttpResponseMessage(httpStatusCode)
            {
                RequestMessage = new HttpRequestMessage(HttpMethod.Get, "https://localhost"),
            };
            responseMessage.Headers.Location = redirectTarget;

            var traceHop = await _processor.Process(responseMessage);

            Assert.IsTrue(traceHop is { RedirectType: RedirectType.Http } && traceHop.RedirectTargetUrl == redirectTarget);

        }
    }
}
