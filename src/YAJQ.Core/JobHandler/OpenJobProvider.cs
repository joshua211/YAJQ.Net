using Microsoft.Extensions.Logging;
using YAJQ.Core.JobHandler.Interfaces;
using YAJQ.Core.Persistence;
using YAJQ.Core.Persistence.Interfaces;

namespace YAJQ.Core.JobHandler;

public class OpenJobProvider : IOpenJobProvider
{
    private readonly ILogger<OpenJobProvider> logger;
    private readonly YAJQOptions options;
    private readonly IJobRepository repository;
    private string? lastId;

    public OpenJobProvider(IJobRepository repository, ILogger<OpenJobProvider> logger, YAJQOptions options)
    {
        this.repository = repository;
        this.logger = logger;
        this.options = options;
    }

    public async Task<PersistedJob> GetNextJobAsync(string handlerId, CancellationTokenSource wakeupTokenSource,
        CancellationToken cancellationToken = default)
    {
        //TODO check last job id == current job id
        PersistedJob? nextJob = null;
        while (nextJob is null)
        {
            var nextPersistedJob = await repository.GetNextJobAsync();
            if (!nextPersistedJob.WasSuccess)
            {
                LogTrace(handlerId, "No job in the database, waiting for {Timeout} ms", options.HandlerTimeout);

                await Task.Delay(options.HandlerTimeout, wakeupTokenSource.Token).ContinueWith(t =>
                {
                    if (t.IsCanceled)
                        LogTrace(handlerId, "Waking up from waiting");
                }, cancellationToken);

                continue;
            }

            if (lastId == nextPersistedJob.Value.Id)
            {
                LogTrace(handlerId, "Throttling for job {Id}", lastId);
                await Task.Delay(options.SameJobThrottling, cancellationToken);
            }

            lastId = nextPersistedJob.Value.Id;

            if (nextPersistedJob.Value.JobType == JobType.Scheduled)
            {
                if (IsHandlerResponsibleForJob(handlerId, nextPersistedJob.Value))
                    //This handler is already responsible for this job, no need to do anything
                    continue;

                if (!IsUpdateOverdue(nextPersistedJob.Value))
                    //Another handler is responsible and keeps updating the job
                    continue;
            }
            else
            {
                if (nextPersistedJob.Value.State != JobState.Pending ||
                    !string.IsNullOrWhiteSpace(nextPersistedJob.Value.ConcurrencyToken) &&
                    !IsUpdateOverdue(nextPersistedJob.Value))
                    //Another handler is already handling this job or it was already handled
                    continue;

                //TODO handle instant job that is already handled but processing takes longer than MaxOverdueTimeout
            }

            nextJob = nextPersistedJob.Value;
        }

        return nextJob;
    }

    private bool IsHandlerResponsibleForJob(string handlerId, PersistedJob job)
    {
        return job.ConcurrencyToken == handlerId;
    }

    private bool IsUpdateOverdue(PersistedJob job)
    {
        return DateTimeOffset.Now > job.LastUpdated.AddSeconds(options.MaxOverdueTimeout);
    }

    private void LogTrace(string handlerId, string message, params object[] args)
    {
        logger.LogTrace($"[{handlerId}]: " + message, args);
    }
}