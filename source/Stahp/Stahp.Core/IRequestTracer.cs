using System.Diagnostics.CodeAnalysis;

namespace Stahp.Core
{
    public interface IRequestTracer
    {
        IAsyncEnumerable<TraceHop> TraceUrlAsync(Uri url);
    }
}