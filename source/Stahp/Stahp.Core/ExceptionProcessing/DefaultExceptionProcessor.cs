namespace Stahp.Core.ExceptionProcessing
{
    internal class DefaultExceptionProcessor : ExceptionProcessorBase, IExceptionProcessor
    {
        public override Task<bool> CanProcess(Exception exception, HttpRequestMessage httpRequestMessage)
        {
            return Task.FromResult(true);
        }

        public override Task<TraceHop> Process(Exception exception, HttpRequestMessage httpRequestMessage)
        {
            return Task.FromResult(new TraceHop(httpRequestMessage.RequestUri!, exception));
        }
    }
}
