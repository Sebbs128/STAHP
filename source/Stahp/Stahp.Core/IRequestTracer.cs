using System.Diagnostics.CodeAnalysis;

namespace Stahp.Core
{
    public interface IRequestTracer
    {
        Task<TraceResult> TraceUrl([NotNull] Uri url);
    }
}