using Fastjob.Core.JobProcessor;

namespace Fastjob.Core.JobHandler;

public class RoundRobinProcessorSelectionStrategy : IProcessorSelectionStrategy
{
    private readonly List<IJobProcessor> availableProcessors;
    private int cursor;

    public RoundRobinProcessorSelectionStrategy()
    {
        availableProcessors = new List<IJobProcessor>();
        cursor = 0;
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

    public async Task<IJobProcessor?> GetNextProcessorAsync()
    {
        if (!availableProcessors.Any())
            return null;

        if (cursor >= availableProcessors.Count)
            cursor = 0;

        var next = availableProcessors[cursor];
        cursor++;

        while (next.IsProcessing)
        {
            await Task.Delay(10);
        }

        return next;
    }
}