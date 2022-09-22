using YAJQ.Core.JobProcessor;

namespace YAJQ.Core.JobHandler;

public interface IProcessorSelectionStrategy
{
    void AddProcessor(IJobProcessor processor);
    void RemoveProcessor();
    Task<IJobProcessor?> GetNextProcessorAsync();
    IEnumerable<string> GetProcessorIds();
}