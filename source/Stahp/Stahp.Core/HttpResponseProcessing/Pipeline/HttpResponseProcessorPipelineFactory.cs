using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stahp.Core.HttpResponseProcessing.Pipeline
{
    internal class HttpResponseProcessorPipelineFactory
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IOptionsMonitor<PipelineOptions> _optionsMonitor;

        public HttpResponseProcessorPipelineFactory(IServiceProvider serviceProvider, IOptionsMonitor<PipelineOptions> optionsMonitor)
        {
            _serviceProvider = serviceProvider;
            _optionsMonitor = optionsMonitor;
        }

        public IHttpResponseProcessor Create()
        {
            var options = _optionsMonitor.CurrentValue;
            var builder = _serviceProvider.GetRequiredService<HttpResponseProcessorBuilder>();

            foreach (var configure in options.BuilderActions)
            {
                configure(builder);
            }

            return builder.Build();
        }
    }
}
