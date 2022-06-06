
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stahp.Core.ExceptionProcessing
{
    internal abstract class ExceptionProcessorBase : IExceptionProcessor
    {
        public abstract Task<bool> CanProcess(Exception exception, HttpRequestMessage httpRequestMessage);
        public abstract Task<TraceHop> Process(Exception exception, HttpRequestMessage httpRequestMessage);
    }
}
