using YAJQ.Core.JobProcessor.Interfaces;

namespace YAJQ.Core.JobHandler.Interfaces;

public interface IProcessorSelectionStrategy
{
    void AddProcessor(IJobProcessor processor);
    void RemoveProcessor();
    Task<IJobProcessor?> GetNextProcessorAsync();
    IEnumerable<string> GetProcessorIds();
}