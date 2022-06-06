namespace Stahp.Core.ExceptionProcessing
{
    public interface IExceptionProcessor
    {
        Task<bool> CanProcess(Exception exception, HttpRequestMessage httpRequestMessage);
        Task<TraceHop> Process(Exception exception, HttpRequestMessage httpRequestMessage);
    }
}