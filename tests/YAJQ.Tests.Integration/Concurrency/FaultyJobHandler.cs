using System;
using System.Threading;
using System.Threading.Tasks;
using YAJQ.Core;
using YAJQ.Core.JobHandler;
using YAJQ.Core.Persistence;

namespace YAJQ.Tests.Integration.Concurrency;

public class FaultyJobHandler : IJobHandler
{
    private readonly YAJQOptions options;
    private readonly IJobRepository repository;

    public FaultyJobHandler(IJobRepository repository, YAJQOptions options)
    {
        this.repository = repository;
        this.options = options;
    }

    public string HandlerId => "FAULTY";

    public async Task Start(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            var nextJob = await WaitForNextJobAsync(cancellationToken);

            var result = await repository.TryGetAndMarkJobAsync(nextJob, HandlerId);
            if (result.WasSuccess)
                break;
        }
    }

    private async Task<PersistedJob> WaitForNextJobAsync(CancellationToken cancellationToken)
    {
        PersistedJob? nextJob = null;
        while (nextJob is null)
        {
            var nextPersistedJob = await repository.GetNextJobAsync();
            if (!nextPersistedJob.WasSuccess)
            {
                await Task.Delay(10);

                continue;
            }

            if (nextPersistedJob.Value.JobType == JobType.Scheduled)
            {
                if (IsHandlerResponsibleForJob(nextPersistedJob.Value))
                    //This handler is already responsible for this job, no need to do anything
                    continue;

                if (!IsUpdateOverdue(nextPersistedJob.Value))
                    //Another handler is responsible and keeps updating the job
                    continue;
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(nextPersistedJob.Value.ConcurrencyToken) ||
                    nextPersistedJob.Value.State != JobState.Pending)
                    //Another handler is already handling this job or it was already handled
                    continue;
            }

            nextJob = nextPersistedJob.Value;
        }

        return nextJob;
    }

    private bool IsHandlerResponsibleForJob(PersistedJob job) =>
        job.ConcurrencyToken == HandlerId;

    private bool IsUpdateOverdue(PersistedJob job) =>
        DateTimeOffset.Now > job.LastUpdated.AddSeconds(options.MaxOverdueTimeout);
}