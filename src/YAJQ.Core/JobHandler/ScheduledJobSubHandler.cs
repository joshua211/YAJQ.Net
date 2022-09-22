using Microsoft.Extensions.Logging;
using YAJQ.Core.JobProcessor;
using YAJQ.Core.Persistence;

namespace YAJQ.Core.JobHandler;

public class ScheduledJobSubHandler : IScheduledJobSubHandler
{
    private readonly YAJQOptions options;
    private readonly List<PersistedJob> scheduledJobs;
    private readonly IJobRepository repository;
    private readonly ILogger<IScheduledJobSubHandler> logger;

    public ScheduledJobSubHandler(YAJQOptions options, IJobRepository repository,
        ILogger<IScheduledJobSubHandler> logger)
    {
        this.options = options;
        this.scheduledJobs = new List<PersistedJob>();
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

    private async Task HandleAsync(string handlerId, IProcessorSelectionStrategy selectionStrategy,
        Func<PersistedJob, IJobProcessor, CancellationToken, Task> processJob,
        CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            foreach (var job in scheduledJobs.ToList())
            {
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
            }

            await Task.Delay(options.ScheduledJobTimerInterval, cancellationToken);
        }
    }


    public void AddScheduledJob(PersistedJob job)
    {
        scheduledJobs.Add(job);
    }

    private void LogWarning(string handlerId, string message, params object[] args) =>
        logger.LogWarning($"[{handlerId}]: " + message, args);
}