using AngleSharp;

using Moq;

using Stahp.Core.HostTypes;
using Stahp.Core.HttpResponseProcessing;

using System.Net;

namespace Stahp.Core.Tests
{
    [TestClass]
    public class HtmlRedirectProcessorTests
    {
        private readonly HtmlRedirectProcessor _processor;

        const string RedirectUrl = "https://www.mozilla.org";

        const string EmptyHtml = "<html></html>";
        const string EmptyHead = "<html><head></head></html>";
        const string EmptyBody = "<html><body></body></html>";
        const string EmptyHeadAndBody = "<html><head></head><body></body></html>";
        const string MetaCharset = """<html><head><meta charset="utf-8" /></head></html>""";
        const string MetaAuthor = """<html><head><meta name="author" content="Sebbs128" /></head></html>""";
        const string MetaHttpEquivContentType = """<html><head><meta http-equiv="content-type" content="text/html; charset=utf-8" /></head></html>""";
        const string MetaHttpEquivRefreshMozilla = $$"""<html><head><meta http-equiv="refresh" content="3;url={{RedirectUrl}}" /></head></html>""";

        public HtmlRedirectProcessorTests()
        {
            IConfiguration config = Configuration.Default;

            var hostFactoryMock = new Mock<IHostFactory>();
            hostFactoryMock.Setup(f => f.GetHost(It.IsAny<Uri>()))
                .Returns<Uri>(uri =>
                {
                    var host = new Mock<IHost>();

                    host.SetupGet(h => h.HostName)
                        .Returns(uri.Host);
                    
                    host.SetupGet(h => h.HostUrl)
                        .Returns(uri.ToString());
                    
                    return Task.FromResult(host.Object);
                });

            _processor = new HtmlRedirectProcessor(config, hostFactoryMock.Object);
        }

        [TestMethod]
        [DataRow("", DisplayName = "Empty String")]
        [DataRow(EmptyHtml, DisplayName = nameof(EmptyHtml))]
        [DataRow(EmptyHead, DisplayName = nameof(EmptyHead))]
        [DataRow(EmptyBody, DisplayName = nameof(EmptyBody))]
        [DataRow(EmptyHeadAndBody, DisplayName = nameof(EmptyHeadAndBody))]
        [DataRow(MetaCharset, DisplayName = nameof(MetaCharset))]
        [DataRow(MetaAuthor, DisplayName = nameof(MetaAuthor))]
        [DataRow(MetaHttpEquivContentType, DisplayName = nameof(MetaHttpEquivContentType))]
        public async Task Process_NoMetaRefreshUrlTags_ReturnsNull(string responseBody)
        {
            HttpResponseMessage responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                RequestMessage = new HttpRequestMessage(HttpMethod.Get, "https://localhost"),
                Content = new StringContent(responseBody)
            };

            var result = await _processor.Process(responseMessage);

            Assert.IsNull(result);
        }

        [TestMethod]
        [DataRow(MetaHttpEquivRefreshMozilla, RedirectUrl, DisplayName = nameof(MetaHttpEquivRefreshMozilla))]
        public async Task CanProcess_MetaRefreshUrl_ReturnsRedirectToUrl(string responseBody, string expectedRedirectTargetUrl)
        {
            HttpResponseMessage responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                RequestMessage = new HttpRequestMessage(HttpMethod.Get, "https://localhost"),
                Content = new StringContent(responseBody)
            };

            Uri expectedRedirectTarget = new Uri(expectedRedirectTargetUrl);

            var traceHop = await _processor.Process(responseMessage);

            Assert.IsTrue(traceHop is { RedirectType: RedirectType.HtmlMeta } && traceHop.RedirectTargetUrl == expectedRedirectTarget);
        }
    }
}