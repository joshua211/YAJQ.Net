using Fastjob.Core.JobProcessor;

namespace Fastjob.Core.JobHandler;

public class RoundRobinProcessorSelectionStrategy : IProcessorSelectionStrategy
{
    private readonly List<IJobProcessor> availableProcessors;
    private Stack<IJobProcessor> processorStack;

    public RoundRobinProcessorSelectionStrategy()
    {
        availableProcessors = new List<IJobProcessor>();
        processorStack = new Stack<IJobProcessor>();
    }

    public void AddProcessor(IJobProcessor processor)
    {
        availableProcessors.Add(processor);
    }

    public void RemoveProcessor()
    {
        if (availableProcessors.Count > 0)
            availableProcessors.RemoveAt(availableProcessors.Count - 1);
    }

    public async Task<IJobProcessor> GetNextProcessorAsync()
    {
        if (processorStack.Count == 0)
            processorStack = new Stack<IJobProcessor>(availableProcessors);

        var next = processorStack.Pop();
        while (next.IsProcessing)
        {
            await Task.Delay(10);
        }

        return next;
    }
}