using AngleSharp;

using Stahp.Core.HttpResponseProcessing;

using System.Net;

namespace Stahp.Core.Tests
{
    [TestClass]
    public class JsRedirectProcessorTests
    {
        private readonly JsRedirectProcessor _processor;

        const string RedirectUrl = "https://www.mozilla.org";

        const string EmptyHtml = "<html></html>";
        const string EmptyBody = "<html><body></body></html>";
        const string SimpleHeadHrefMozilla = """
            <html>
                <head>
                    <script type="text/javascript">
                        window.location.href = "https://www.mozilla.org";
                    </script>
                </head>
            </html>
            """;
        const string SimpleBodyHrefMozilla = """
            <html>
                <body>
                    <script type="text/javascript">
                        window.location.href = "https://www.mozilla.org";
                    </script>
                </body>
            </html>
            """;

        // legacy event handlers registered through onX
        const string Document_LegacyEvent_ReadyStateComplete_Function_HrefMozilla = $$"""
            <html>
                <head>
                    <script type="text/javascript">
                        document.onreadystatechange = function() {
                            if (document.readyState == "complete")
                                window.location.href = "{{RedirectUrl}}";
                        };
                    </script>
                </head>
            </html>
            """;
        const string Window_LegacyEvent_Load_Function_HrefMozilla = $$"""
            <html>
                <head>
                    <script type="text/javascript">
                        window.onload = function() {
                            window.location.href = "{{RedirectUrl}}";
                        };
                    </script>
                </head>
            </html>
            """;
        const string Window_LegacyEvent_PageShow_Function_HrefMozilla = $$"""
            <html>
                <head>
                    <script type="text/javascript">
                        window.onpageshow = function() {
                            window.location.href = "{{RedirectUrl}}";
                        };
                    </script>
                </head>
            </html>
            """;

        // event handlers registered through addEventListener(, function() { })
        const string Document_EventListener_DomContentLoaded_Function_HrefMozilla = $$"""
            <html>
                <head>
                    <script type="text/javascript">
                        document.addEventListener("DOMContentLoaded", function() {
                            window.location.href = "{{RedirectUrl}}";
                        });
                    </script>
                </head>
            </html>
            """;
        const string Document_EventListener_ReadyStateComplete_Function_HrefMozilla = $$"""
            <html>
                <head>
                    <script type="text/javascript">
                        document.addEventListener("readystatechange", function() {
                            if (document.readyState == "complete")
                                window.location.href = "{{RedirectUrl}}";
                        });
                    </script>
                </head>
            </html>
            """;
        const string Window_EventListener_Load_Function_HrefMozilla = $$"""
            <html>
                <head>
                    <script type="text/javascript">
                        window.addEventListener("load", function() {
                            window.location.href = "{{RedirectUrl}}";
                        });
                    </script>
                </head>
            </html>
            """;
        const string Window_EventListener_PageShow_Function_HrefMozilla = $$"""
            <html>
                <head>
                    <script type="text/javascript">
                        window.addEventListener("pageshow", function() {
                            window.location.href = "{{RedirectUrl}}";
                        });
                    </script>
                </head>
            </html>
            """;

        // event handlers registered through addEventListener(, (event) => { })
        // not supported by AngleSharp.Js v0.15.0, as it's Jint dependency only implements ECMA 5.1
        const string Document_EventListener_DomContentLoaded_Arrow_HrefMozilla = $$"""
            <html>
                <head>
                    <script type="text/javascript">
                        document.addEventListener("DOMContentLoaded", (event) => {
                            window.location.href = "{{RedirectUrl}}";
                        });
                    </script>
                </head>
            </html>
            """;
        const string Document_EventListener_ReadyStateComplete_Arrow_HrefMozilla = $$"""
            <html>
                <head>
                    <script type="text/javascript">
                        document.addEventListener("readystatechange", (event) => {
                            if (document.readyState == "complete")
                                window.location.href = "{{RedirectUrl}}";
                        });
                    </script>
                </head>
            </html>
            """;
        const string Window_EventListener_Load_Arrow_HrefMozilla = $$"""
            <html>
                <head>
                    <script type="text/javascript">
                        window.addEventListener("load", (event) => {
                            window.location.href = "{{RedirectUrl}}";
                        });
                    </script>
                </head>
            </html>
            """;
        const string Window_EventListener_PageShow_Arrow_HrefMozilla = $$"""
            <html>
                <head>
                    <script type="text/javascript">
                        window.addEventListener("pageshow", (event) => {
                            window.location.href = "{{RedirectUrl}}";
                        });
                    </script>
                </head>
            </html>
            """;

        public JsRedirectProcessorTests()
        {
            IConfiguration config = Configuration.Default
                .WithJs()
                .WithEventLoop();

            var hostFactoryMock = Mocks.HostFactory;

            _processor = new JsRedirectProcessor(config, hostFactoryMock);
        }

        [TestMethod]
        [DataRow("", DisplayName = "Empty String")]
        [DataRow(EmptyHtml, DisplayName = nameof(EmptyHtml))]
        [DataRow(EmptyBody, DisplayName = nameof(EmptyBody))]
        public async Task Process_NoScriptTags_ReturnsNull(string responseBody)
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
        [DataRow(SimpleHeadHrefMozilla, RedirectUrl, DisplayName = nameof(SimpleHeadHrefMozilla))]
        [DataRow(SimpleBodyHrefMozilla, RedirectUrl, DisplayName = nameof(SimpleBodyHrefMozilla))]
        public async Task Process_RedirectingScriptTags_ReturnsRedirectToUrl(string responseBody, string expectedRedirectTargetUrl)
        {
            HttpResponseMessage responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                RequestMessage = new HttpRequestMessage(HttpMethod.Get, "https://localhost"),
                Content = new StringContent(responseBody)
            };

            Uri expectedRedirectTarget = new Uri(expectedRedirectTargetUrl);

            var traceHop = await _processor.Process(responseMessage);

            Assert.IsTrue(traceHop is { RedirectType: RedirectType.JsHref } && traceHop.RedirectTargetUrl == expectedRedirectTarget);
        }

        [TestMethod]
        // disabled due to known issue, waiting for AngleSharp.Js fix or upgrade to use Jint v3
        //[DataRow(Document_LegacyEvent_ReadyStateComplete_Function_HrefMozilla, RedirectUrl, DisplayName = nameof(Document_LegacyEvent_ReadyStateComplete_Function_HrefMozilla))]
        [DataRow(Window_LegacyEvent_Load_Function_HrefMozilla, RedirectUrl, DisplayName = nameof(Window_LegacyEvent_Load_Function_HrefMozilla))]
        [DataRow(Window_LegacyEvent_PageShow_Function_HrefMozilla, RedirectUrl, DisplayName = nameof(Window_LegacyEvent_PageShow_Function_HrefMozilla))]
        
        [DataRow(Document_EventListener_DomContentLoaded_Function_HrefMozilla, RedirectUrl, DisplayName = nameof(Document_EventListener_DomContentLoaded_Function_HrefMozilla))]
        // disabled due to known issues, waiting for AngleSharp.Js to use Jint v3
        //[DataRow(Document_EventListener_ReadyStateComplete_Function_HrefMozilla, RedirectUrl, DisplayName = nameof(Document_EventListener_ReadyStateComplete_Function_HrefMozilla))]
        [DataRow(Window_EventListener_Load_Function_HrefMozilla, RedirectUrl, DisplayName = nameof(Window_EventListener_Load_Function_HrefMozilla))]
        [DataRow(Window_EventListener_PageShow_Function_HrefMozilla, RedirectUrl, DisplayName = nameof(Window_EventListener_PageShow_Function_HrefMozilla))]

        // arrow function tests disabled due to known issues, waiting for AngleSharp.Js to use Jint v3
        //[DataRow(Document_EventListener_DomContentLoaded_Arrow_HrefMozilla, RedirectUrl, DisplayName = nameof(Document_EventListener_DomContentLoaded_Arrow_HrefMozilla))]
        //[DataRow(Document_EventListener_ReadyStateComplete_Arrow_HrefMozilla, RedirectUrl, DisplayName = nameof(Document_EventListener_ReadyStateComplete_Arrow_HrefMozilla))]
        //[DataRow(Window_EventListener_Load_Arrow_HrefMozilla, RedirectUrl, DisplayName = nameof(Window_EventListener_Load_Arrow_HrefMozilla))]
        //[DataRow(Window_EventListener_PageShow_Arrow_HrefMozilla, RedirectUrl, DisplayName = nameof(Window_EventListener_PageShow_Arrow_HrefMozilla))]
        public async Task Process_OnEventRedirectScriptTags_ReturnsRedirectToUrl(string responseBody, string expectedRedirectTargetUrl)
        {
            HttpResponseMessage responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                RequestMessage = new HttpRequestMessage(HttpMethod.Get, "https://localhost"),
                Content = new StringContent(responseBody)
            };

            Uri expectedRedirectTarget = new Uri(expectedRedirectTargetUrl);

            var traceHop = await _processor.Process(responseMessage);

            Assert.IsTrue(traceHop is { RedirectType: RedirectType.JsHref } && traceHop.RedirectTargetUrl == expectedRedirectTarget);
        }
    }
}
