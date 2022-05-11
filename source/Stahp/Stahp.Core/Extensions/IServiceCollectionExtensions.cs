using Microsoft.Extensions.DependencyInjection;

using Whois;

namespace Stahp.Core
{
    public static class IServiceCollectionExtensions
    {
        public static IServiceCollection AddStahpCore(this IServiceCollection services)
        {
            services
                .AddHttpClient<RequestTracer>()
                .ConfigurePrimaryHttpMessageHandler(_ => new HttpClientHandler()
                {
                    AllowAutoRedirect = false
                });
            return services
                .AddTransient(_ =>
                {
                    WhoisLookup? client = new WhoisLookup();
                    client.Options.TimeoutSeconds = 120;
                    return client;
                })
                .AddTransient<IRequestTracer, RequestTracer>();
        }
    }
}
