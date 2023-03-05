using AngleSharp;

using Microsoft.Extensions.DependencyInjection;

using Stahp.Core.ExceptionProcessing;
using Stahp.Core.HostTypes;
using Stahp.Core.HttpResponseProcessing;
using Stahp.Core.HttpResponseProcessing.Pipeline;

using Whois;

namespace Stahp.Core
{
    public static class IServiceCollectionExtensions
    {
        public static IServiceCollection AddStahpCore(this IServiceCollection services)
        {
            services
                .AddHttpClient<IRequestTracer, RequestTracer>()
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
                .AddTransient(_ => new WhoisLookup());
        }

        private static IServiceCollection AddAngleSharp(this IServiceCollection services)
        {
            return services.AddSingleton(Configuration.Default
                .WithJs()
                .WithEventLoop()
                .WithDefaultLoader());
        }

        private static IServiceCollection AddHttpResponseProcessors(this IServiceCollection services)
        {
            // each IHttpResponseProcessor implementation must be
            // - registered in DI
            // - have an action to resolve added to PipelineOptions
            //   - the default processor should be set on PrimaryProcessor
            //   - all others should be added as DelegatingProcessors
            // a pipeline factory will take IServiceProvider and the PipelineOptions
            // RequestTracer takes the pipeline factory, and asks it to build and return the pipeline

            services.Configure<PipelineOptions>(options =>
            {
                options.BuilderActions.Add(b => b.PrimaryProcessor = b.Services.GetRequiredService<DefaultHttpResponseProcessor>());
                // add in the order they should be run
                options.BuilderActions.Add(b => b.DelegatingProcessors.Add(b.Services.GetRequiredService<JsRedirectProcessor>()));
                options.BuilderActions.Add(b => b.DelegatingProcessors.Add(b.Services.GetRequiredService<HtmlRedirectProcessor>()));
                options.BuilderActions.Add(b => b.DelegatingProcessors.Add(b.Services.GetRequiredService<HttpRedirectProcessor>()));
            });

            return services
                .AddTransient<JsRedirectProcessor>()
                .AddTransient<HtmlRedirectProcessor>()
                .AddTransient<HttpRedirectProcessor>()
                .AddTransient<DefaultHttpResponseProcessor>()
                .AddTransient<HttpResponseProcessorBuilder>()
                .AddSingleton<HttpResponseProcessorPipelineFactory>();
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
