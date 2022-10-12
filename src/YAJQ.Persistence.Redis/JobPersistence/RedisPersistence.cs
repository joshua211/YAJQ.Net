﻿using System.Reflection;
using System.Text.Json;
using StackExchange.Redis;
using YAJQ.Core.Archive.Interfaces;
using YAJQ.Core.Common;
using YAJQ.Core.Persistence;
using YAJQ.Core.Persistence.Interfaces;

namespace YAJQ.Persistence.Redis.JobPersistence;

public class RedisPersistence : IJobPersistence, IDisposable
{
    private const string JobListId = "YAJQ.OpenJobs";
    private const string CursorId = "YAJQ.Cursor";
    private const string ChannelId = "YAJQ.Channel";

    private readonly IDatabase database;
    private readonly IHashSerializer hashSerializer;
    private readonly IJobArchive archive;

    public RedisPersistence(IConnectionMultiplexer multiplexer, IHashSerializer hashSerializer,
        IJobArchive archive)
    {
        this.hashSerializer = hashSerializer;
        this.archive = archive;
        database = multiplexer.GetDatabase();
        multiplexer.GetSubscriber().Subscribe(ChannelId, OnNewChannelMessage);
    }

    public event EventHandler<string>? NewJob;

    public async Task<ExecutionResult<Success>> SaveJobAsync(PersistedJob persistedJob)
    {
        var hash = hashSerializer.Serialize(persistedJob);

        var transaction = database.CreateTransaction();
        transaction.ListRightPushAsync(JobListId, persistedJob.Id.Value);
        transaction.HashSetAsync(persistedJob.Id.Value, hash);
        var result = await transaction.ExecuteAsync();
        
        if (result)
            await database.Multiplexer.GetSubscriber().PublishAsync(ChannelId, persistedJob.Id.Value);

        return result ? new Success() : Error.StorageError();
    }

    public async Task<ExecutionResult<PersistedJob>> GetJobAsync(string id)
    {
        var pos = await database.ListPositionAsync(JobListId, id);
        if (pos < 0)
            return Error.NotFound();

        var hash = await database.HashGetAllAsync(id);
        var job = hashSerializer.Deserialize(hash);

        return job is null ? Error.NotFound() : job;
    }

    public async Task<ExecutionResult<PersistedJob>> UpdateStateAsync(PersistedJob persistedJob, JobState expectedState)
    {
        var trans = database.CreateTransaction();
        trans.AddCondition(Condition.HashEqual(persistedJob.Id.Value, "State", (int) expectedState));
        trans.HashSetAsync(persistedJob.Id.Value, new[] {new HashEntry("State", (int) persistedJob.State)});
        var result = await trans.ExecuteAsync();

        return result ? persistedJob : Error.OutdatedUpdate();
    }

    public async Task<ExecutionResult<PersistedJob>> UpdateTokenAsync(PersistedJob persistedJob, string expectedToken)
    {
        var trans = database.CreateTransaction();
        trans.AddCondition(Condition.HashEqual(persistedJob.Id.Value, "ConcurrencyToken", expectedToken));
        trans.HashSetAsync(persistedJob.Id.Value,
            new[] {new HashEntry("ConcurrencyToken", persistedJob.ConcurrencyToken)});
        var result = await trans.ExecuteAsync();

        return result ? persistedJob : Error.OutdatedUpdate();
    }

    public async Task<ExecutionResult<Success>> RemoveJobAsync(string jobId)
    {
        var listRemove = await database.ListRemoveAsync(JobListId, jobId);
        var hashRemove = await database.KeyDeleteAsync(jobId);

        return listRemove > 0 && hashRemove ? new Success() : Error.StorageError();
    }

    public async Task<ExecutionResult<Success>> RemoveAllJobsAsync()
    {
        database.KeyDelete(JobListId);

        return new Success();
    }

    public async Task<ExecutionResult<Success>> ArchiveJobAsync(ArchivedJob job)
    {
        var remove = await RemoveJobAsync(job.Id);
        if (!remove.WasSuccess)
            return remove.Error;

        var arch = await archive.AddToArchiveAsync(job);

        return !arch.WasSuccess ? arch.Error : ExecutionResult<Success>.Success;
    }

    public async Task<ExecutionResult<JobCursor>> IncreaseCursorAsync()
    {
        var cursor = await GetCursorAsync();
        var newCursor = cursor.Value.Increase();

        var trans = database.CreateTransaction();
        trans.AddCondition(
            Condition.StringEqual(CursorId, cursor.Value.CurrentCursor));
        trans.StringSetAsync(CursorId, newCursor.CurrentCursor.ToString());
        
        var result = await trans.ExecuteAsync();

        return result ? newCursor : Error.OutdatedUpdate();
    }

    public async Task<ExecutionResult<PersistedJob>> GetJobAtCursorAsync()
    {
        var cursor = await GetCursorAsync();
        if (!cursor.WasSuccess)
            return cursor.Error;

        if (cursor.Value.MaxCursor == 0)
            return Error.CursorOutOfRange();

        var id = await database.ListGetByIndexAsync(JobListId, cursor.Value.CurrentCursor);
        var job = await GetJobAsync(id);
        await IncreaseCursorAsync();

        return job;
    }

    public async Task<ExecutionResult<JobCursor>> GetCursorAsync()
    {
        await database.StringSetAsync(CursorId, 0, when:When.NotExists);
        
        var current = (int)(await database.StringGetAsync(CursorId));
        var max = (int)(await database.ListLengthAsync(JobListId));
        
        var cursor = JobCursor.With(current, max);

        return cursor;
    }

    private void OnNewChannelMessage(RedisChannel channel, RedisValue value)
    {
        NewJob?.Invoke(this, value);
    }

    public void Dispose()
    {
        database.Multiplexer.GetSubscriber().Unsubscribe(ChannelId, OnNewChannelMessage);
    }
}