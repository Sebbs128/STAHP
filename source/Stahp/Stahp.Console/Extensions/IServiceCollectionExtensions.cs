using Microsoft.Extensions.DependencyInjection;

using Spectre.Console.Cli;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stahp.Console.Extensions
{
    public static class IServiceCollectionExtensions
    {
        public static IServiceCollection AddSpectreCli(this IServiceCollection services, Action<IConfigurator> configurator)
        {
            CommandApp? app = new CommandApp(new TypeRegistrar(services));
            app.Configure(configurator);

            return services.AddSingleton<ICommandApp>(app);
        }

        public static IServiceCollection AddSpectreCli<TDefaultCommand>(this IServiceCollection services, Action<IConfigurator> configurator)
            where TDefaultCommand : class, ICommand
        {
            CommandApp<TDefaultCommand>? app = new CommandApp<TDefaultCommand>(new TypeRegistrar(services));
            app.Configure(configurator);

            return services.AddSingleton<ICommandApp>(app);
        }
    }
}
