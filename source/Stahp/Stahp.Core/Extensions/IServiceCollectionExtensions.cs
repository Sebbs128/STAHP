using Microsoft.Extensions.DependencyInjection;

using Stahp.Core.HostTypes;
using Stahp.Core.HttpResponseProcessing;

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
                .AddMemoryCache()
                .AddTransient<IHostFactory, HostFactory>()
                .AddTransient<IHttpResponseProcessor, HtmlRedirectProcessor>()
                .AddTransient<IHttpResponseProcessor, HttpRedirectProcessor>()
                // DefaultHttpResponseProcessor must be added last, as its CanProcess() will always return true
                .AddTransient<IHttpResponseProcessor, DefaultHttpResponseProcessor>()
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
