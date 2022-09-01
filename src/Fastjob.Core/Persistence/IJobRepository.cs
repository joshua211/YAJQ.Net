﻿using Fastjob.Core.Common;
using Fastjob.Core.Interfaces;

namespace Fastjob.Core.Persistence;

public interface IJobRepository
{
    public event EventHandler<JobEvent> Update;
    Task<ExecutionResult<string>> AddJobAsync(IJobDescriptor descriptor, string? id = null);
    Task<ExecutionResult<PersistedJob>> GetNextJobAsync();
    Task<ExecutionResult<PersistedJob>> GetJobAsync(string id);
    Task<ExecutionResult<PersistedJob>> TryGetAndMarkJobAsync(string jobId, string concurrencyMark);
    Task<ExecutionResult<Success>> CompleteJobAsync(string jobId, bool wasSuccess = true);
}