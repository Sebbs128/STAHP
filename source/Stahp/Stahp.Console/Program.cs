using Microsoft.Extensions.Hosting;

using Spectre.Console.Cli;

using Stahp.Console;
using Stahp.Console.Extensions;
using Stahp.Core;

await new HostBuilder()
    .ConfigureServices(services =>
    {
        services
            .AddStahpCore()
            .AddSpectreCli<RequestTracerCommand>(config =>
            {
                config.SetApplicationName("Stahp");
            });
    })
    .RunSpectreAsync(args);