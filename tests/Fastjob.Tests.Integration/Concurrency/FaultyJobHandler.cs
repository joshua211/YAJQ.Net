﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Fastjob.Core;
using Fastjob.Core.JobHandler;
using Fastjob.Core.Persistence;

namespace Fastjob.Tests.Integration.Concurrency;

public class FaultyJobHandler : IJobHandler
{
    private readonly FastjobOptions options;
    private readonly IJobRepository repository;

    public FaultyJobHandler(IJobRepository repository, FastjobOptions options)
    {
        this.repository = repository;
        this.options = options;
    }

    public string HandlerId => "FAULTY";

    public async Task Start(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            var nextJob = await WaitForNextJobIdAsync(cancellationToken);

            var result = await repository.TryGetAndMarkJobAsync(nextJob, HandlerId);
            if (result.WasSuccess)
                break;
        }
    }

    private async Task<string> WaitForNextJobIdAsync(CancellationToken cancellationToken)
    {
        string? nextJob = null;
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

            nextJob = nextPersistedJob.Value.Id;
        }

        return nextJob;
    }

    private bool IsHandlerResponsibleForJob(PersistedJob job) =>
        job.ConcurrencyToken == HandlerId;

    private bool IsUpdateOverdue(PersistedJob job) =>
        DateTimeOffset.Now > job.LastUpdated.AddSeconds(options.MaxOverdueTimeout);
}