using System;
using Fastjob.Core.Persistence;

namespace Fastjob.Tests.Unit;

public abstract class TestBase
{
    public string JobId => "XXXXXXXXXXXXXXXXX";
    public PersistedJob PersistedSyncJob(string? id = null) => new (id ?? JobId, TestService.SyncDescriptor(), string.Empty);
    public PersistedJob PersistedAsyncJob(string? id = null) => new (id ?? JobId, TestService.AsyncDescriptor(), string.Empty);
    public PersistedJob PersistedExcJob(string? id = null) => new (id ?? JobId, TestService.ExceptionDescriptor(), string.Empty);
}