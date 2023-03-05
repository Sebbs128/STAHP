namespace Stahp.Core.HttpResponseProcessing.Pipeline
{
    internal class PipelineOptions
    {
        public IList<Action<HttpResponseProcessorBuilder>> BuilderActions { get; } = new List<Action<HttpResponseProcessorBuilder>>();
    }
}
