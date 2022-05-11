using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Spectre.Console.Cli;

namespace Stahp.Console.Extensions
{
    public static class IHostExtensions
    {
        public static Task<int> RunSpectreAsync(this IHost host, string[] args)
        {
            ICommandApp? app = host.Services.GetService<ICommandApp>();
            if (app == null)
            {
                throw new InvalidOperationException("Command application has not been configured.");
            }

            return app.RunAsync(args);
        }

        public static Task<int> RunSpectreAsync(this IHostBuilder hostBuilder, string[] args)
        {
            return hostBuilder
                .Build()
                .RunSpectreAsync(args);
        }
    }
}
