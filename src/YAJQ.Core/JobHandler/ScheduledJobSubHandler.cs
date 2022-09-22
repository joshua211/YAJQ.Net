using Microsoft.Extensions.Logging;
using YAJQ.Core.JobHandler.Interfaces;
using YAJQ.Core.JobProcessor;
using YAJQ.Core.JobProcessor.Interfaces;
using YAJQ.Core.Persistence;
using YAJQ.Core.Persistence.Interfaces;

namespace YAJQ.Core.JobHandler;

public class ScheduledJobSubHandler : IScheduledJobSubHandler
{
    private readonly ILogger<IScheduledJobSubHandler> logger;
    private readonly YAJQOptions options;
    private readonly IJobRepository repository;
    private readonly List<PersistedJob> scheduledJobs;

    public ScheduledJobSubHandler(YAJQOptions options, IJobRepository repository,
        ILogger<IScheduledJobSubHandler> logger)
    {
        this.options = options;
        scheduledJobs = new List<PersistedJob>();
        this.repository = repository;
        this.logger = logger;
    }

    public void Start(string handlerId, IProcessorSelectionStrategy selectionStrategy,
        Func<PersistedJob, IJobProcessor, CancellationToken, Task> processJob,
        CancellationToken cancellationToken = default)
    {
        Task.Run(() => HandleAsync(handlerId, selectionStrategy, processJob, cancellationToken), cancellationToken)
            .ContinueWith(_ => { });
    }


    public void AddScheduledJob(PersistedJob job)
    {
        scheduledJobs.Add(job);
    }

    private async Task HandleAsync(string handlerId, IProcessorSelectionStrategy selectionStrategy,
        Func<PersistedJob, IJobProcessor, CancellationToken, Task> processJob,
        CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            foreach (var job in scheduledJobs.ToList())
                if (job.ScheduledTime < DateTimeOffset.Now)
                {
                    var processor = await selectionStrategy.GetNextProcessorAsync();

                    await processJob(job, processor, cancellationToken);
                    scheduledJobs.Remove(job);
                }
                else
                {
                    var result = await repository.RefreshTokenAsync(job.Id, handlerId);
                    if (!result.WasSuccess)
                        LogWarning("Failed to refresh claimed job with Id {Id}", job.Id);
                }

            await Task.Delay(options.ScheduledJobTimerInterval, cancellationToken);
        }
    }

    private void LogWarning(string handlerId, string message, params object[] args)
    {
        logger.LogWarning($"[{handlerId}]: " + message, args);
    }
}