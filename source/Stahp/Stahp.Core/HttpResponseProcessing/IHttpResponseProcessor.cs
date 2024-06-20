namespace Stahp.Core.HttpResponseProcessing
{
    public interface IHttpResponseProcessor
    {
        Task<TraceHop?> Process(HttpResponseMessage httpResponseMessage);
    }
}
