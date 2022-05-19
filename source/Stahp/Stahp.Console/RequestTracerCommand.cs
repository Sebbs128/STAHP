using Spectre.Console;
using Spectre.Console.Cli;
using Spectre.Console.Rendering;

using Stahp.Core;
using Stahp.Core.HostTypes;

using System.ComponentModel;
using System.Net;

namespace Stahp.Console
{
    public class RequestTracerCommand : AsyncCommand<RequestTracerCommand.RequestTracerSettings>
    {
        private readonly IRequestTracer _requestTracer;
        private readonly IAnsiConsole _console;

        public class RequestTracerSettings : CommandSettings
        {
            [CommandOption("-u|--url <URL>")]
            [Description("The URL to start a trace for.")]
            [DefaultValue(null)]
            public string? Url { get; set; }

            public override ValidationResult Validate()
            {
                return !string.IsNullOrWhiteSpace(Url) && !Uri.TryCreate(Url, UriKind.Absolute, out Uri? _)
                    ? ValidationResult.Error("Url must be a valid URL")
                    : ValidationResult.Success();
            }
        }

        public RequestTracerCommand(IRequestTracer requestTracer, IAnsiConsole console)
        {
            _requestTracer = requestTracer;
            _console = console;
        }

        public override async Task<int> ExecuteAsync(CommandContext context, RequestTracerSettings settings)
        {
            if (!string.IsNullOrWhiteSpace(settings.Url))
            {
                // due to validation, Url should be a valid Uri

                await DoUrlTracing(new Uri(settings.Url));

                return 0;
            }

            do
            {
                string? url = _console.Prompt(
                    new TextPrompt<string>("Url to trace:")
                        .PromptStyle("cyan")
                        .Validate(input =>
                        {
                            return !string.IsNullOrWhiteSpace(input) && Uri.TryCreate(input, UriKind.Absolute, out Uri? _);
                        })
                        .ValidationErrorMessage("Not a valid web address."));

                await DoUrlTracing(new Uri(url));

            } while (_console.Confirm("Run again?"));

            return 0;
        }

        public async Task DoUrlTracing(Uri uri)
        {
            // can't currently combine dynamic display (ie. Spectre.Console.Status()) with static rendering such as markup and tables
            // see https://github.com/spectreconsole/spectre.console/issues/295#issuecomment-797230737
            // HACK: we can have a Spectre.Console.Status() indicator by creating one that only runs until the next hop is ready
            CancellationTokenSource cts = DisplayCancellableStatusIndicator();
            
            await foreach (TraceHop? traceHop in _requestTracer.TraceUrlAsync(uri))
            {
                cts.Cancel();
                PrintHopDetails(traceHop);

                cts = DisplayCancellableStatusIndicator();
            }
        }

        private CancellationTokenSource DisplayCancellableStatusIndicator()
        {
            CancellationTokenSource cts = new CancellationTokenSource();
            // don't await, because the caller needs to tell the status indicator when to stop
            _console.Status()
                .StartAsync("Tracing web requests...", async ctx =>
                {
                    while (!cts.Token.IsCancellationRequested)
                    {
                        await Task.Delay(1000, cts.Token);
                    }
                });
            return cts;
        }

        private void PrintHopDetails(TraceHop traceHop)
        {
            _console.Markup("[{0}]{1}[/]  ",
                traceHop.HttpStatusCode switch
                {
                    < HttpStatusCode.Moved => "green",
                    (>= HttpStatusCode.Moved) and (<= HttpStatusCode.PermanentRedirect) => "orange1",
                    _ => "red"
                },
                ((int)traceHop.HttpStatusCode).ToString());
            _console.MarkupLine(traceHop.Url.ToString());

            _console.Write(PrintHost(traceHop.DomainHost));

            if (traceHop.Redirects)
            {
                _console.Markup(traceHop.HttpStatusCode switch
                {
                    (>= HttpStatusCode.Moved) and (<= HttpStatusCode.PermanentRedirect) =>
                        $"HTTP {(int)traceHop.HttpStatusCode} ({traceHop.HttpStatusCode})",
                    _ => "HTML"
                });
                _console.MarkupLine(" Redirects to...");

                _console.Write(new Rule());
            }
        }

        private IRenderable PrintHost(IHost? domainHost)
        {
            Table? table = new Table()
                .Border(TableBorder.Minimal);

            List<TableColumn>? columns = new List<TableColumn>()
            {
                new TableColumn("Host"),
                new TableColumn("Host Site") { NoWrap = true },
            };

            List<IRenderable>? rowValues = new List<IRenderable>()
            {
                new Markup(domainHost?.HostName ?? string.Empty),
                new Markup(domainHost?.HostUrl ?? string.Empty)
            };

            switch (domainHost)
            {
                case AzureStorageBlobHost azureStorageBlobHost:
                    columns.AddRange(new[]
                    {
                        new TableColumn("Storage Account"),
                        new TableColumn("Container")
                    });
                    rowValues.AddRange(new[]
                    {
                        new Markup(azureStorageBlobHost.AccountName),
                        new Markup(azureStorageBlobHost.ContainerName)
                    });
                    break;
                case AmazonS3BucketHost amazonS3BucketHost:
                    columns.AddRange(new[]
                    {
                        new TableColumn("Contact"),
                        new TableColumn("Region"),
                        new TableColumn("Bucket")
                    });
                    rowValues.AddRange(new[]
                    {
                        new Markup(amazonS3BucketHost.AbuseContact),
                        new Markup(amazonS3BucketHost.Region),
                        new Markup(amazonS3BucketHost.BucketName)
                    });
                    break;
                default:
                    columns.AddRange(new[]
                    {
                        new TableColumn("Contact"),
                    });
                    rowValues.Add(new Markup(domainHost?.AbuseContact ?? string.Empty));
                    break;
            }

            return table
                .AddColumns(columns.ToArray())
                .AddRow(rowValues);
        }
    }
}
