using System.Net.Sockets;

namespace Stahp.Core.ExceptionProcessing
{
    internal class NoSuchHostProcessor : ExceptionProcessorBase, IExceptionProcessor
    {
        public override Task<bool> CanProcess(Exception exception, HttpRequestMessage httpRequestMessage)
        {
            return Task.FromResult(exception is HttpRequestException httpException
                && httpException.InnerException is SocketException socketException
                && "No such host is known.".Equals(socketException.Message, StringComparison.OrdinalIgnoreCase));
        }

        public override Task<TraceHop> Process(Exception exception, HttpRequestMessage httpRequestMessage)
        {
            return Task.FromResult(new TraceHop(httpRequestMessage.RequestUri!, exception.InnerException!));
        }
    }
}
