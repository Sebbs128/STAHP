using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stahp.Core.HttpResponseProcessing.Pipeline
{
    internal class HttpResponseProcessorBuilder
    {
        public IServiceProvider Services { get; }

        public IHttpResponseProcessor? PrimaryProcessor { get; set; }
        public IList<DelegatingHttpResponseProcessor> DelegatingProcessors { get; } = new List<DelegatingHttpResponseProcessor>();

        public HttpResponseProcessorBuilder(IServiceProvider serviceProvider)
        {
            Services = serviceProvider;
        }

        public IHttpResponseProcessor Build()
        {
            if (PrimaryProcessor is null)
                throw new InvalidOperationException("PrimaryProcessor is null");

            IHttpResponseProcessor next = PrimaryProcessor;

            // order in the list should be the order they are to be run
            // so we need to set up the delegation in the reverse order
            for (int i = DelegatingProcessors.Count - 1; i >= 0; i--)
            {
                DelegatingHttpResponseProcessor? processor = DelegatingProcessors[i];
                
                if (processor is null)
                    throw new InvalidOperationException("Additional processor is null.");

                processor.InnerProcessor = next;
                next = processor;
            }

            return next;
        }
    }
}
