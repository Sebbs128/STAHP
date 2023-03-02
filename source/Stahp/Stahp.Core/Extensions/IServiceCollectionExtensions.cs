using AngleSharp;

using Microsoft.Extensions.DependencyInjection;

using Stahp.Core.ExceptionProcessing;
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
                .AddAngleSharp()
                .AddHttpResponseProcessors()
                .AddExceptionProcessors()
                .AddTransient(_ => new WhoisLookup())
                .AddTransient<IRequestTracer, RequestTracer>();
        }

        private static IServiceCollection AddAngleSharp(this IServiceCollection services)
        {
            return services.AddSingleton(Configuration.Default
                .WithJs()
                .WithDefaultLoader());
        }

        private static IServiceCollection AddHttpResponseProcessors(this IServiceCollection services)
        {
            return services
                .AddTransient<IHttpResponseProcessor, JsRedirectProcessor>()
                .AddTransient<IHttpResponseProcessor, HtmlRedirectProcessor>()
                .AddTransient<IHttpResponseProcessor, HttpRedirectProcessor>()
                // DefaultHttpResponseProcessor must be added last, as its CanProcess() will always return true
                .AddTransient<IHttpResponseProcessor, DefaultHttpResponseProcessor>();
        }

        private static IServiceCollection AddExceptionProcessors(this IServiceCollection services)
        {
            return services
                .AddTransient<IExceptionProcessor, NoSuchHostProcessor>()
                // DefaultExceptionProcessor must be added last, as its CanProcess() will always return true
                .AddTransient<IExceptionProcessor, DefaultExceptionProcessor>();
        }
    }
}
