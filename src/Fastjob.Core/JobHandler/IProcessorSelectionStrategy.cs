using Fastjob.Core.JobProcessor;

namespace Fastjob.Core.JobHandler;

public interface IProcessorSelectionStrategy
{
    void AddProcessor(IJobProcessor processor);
    void RemoveProcessor();
    Task<IJobProcessor?> GetNextProcessorAsync();
}