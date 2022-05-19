using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stahp.Core.HttpResponseProcessing
{
    public interface IHttpResponseProcessor
    {
        Task<bool> CanProcess(HttpResponseMessage httpResponseMessage);

        Task<TraceHop> Process(HttpResponseMessage httpResponseMessage);
    }
}
