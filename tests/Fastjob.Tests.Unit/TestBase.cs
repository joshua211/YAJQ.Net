using System;
using Fastjob.Core.Persistence;
using Fastjob.Core.Utils;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Fastjob.Tests.Unit;

public abstract class TestBase
{
    protected TestService service;
    protected IJobRepository fakeRepository;
    protected IJobPersistence fakePersistence;
    protected IModuleHelper moduleHelper;
    protected ILogger logger;

    public TestBase()
    {
        fakePersistence = Substitute.For<IJobPersistence>();
        fakeRepository = Substitute.For<IJobRepository>();
        moduleHelper = new ModuleHelper();
        logger = Substitute.For<ILogger>();
        service = new TestService();
    }

    public string JobId => "XXXXXXXXXXXXXXXXX";

    public PersistedJob PersistedSyncJob(string? id = null) =>
        new(id ?? JobId, TestService.SyncDescriptor(), string.Empty);

    public PersistedJob PersistedAsyncJob(string? id = null) =>
        new(id ?? JobId, TestService.AsyncDescriptor(), string.Empty);

    public PersistedJob PersistedExcJob(string? id = null) =>
        new(id ?? JobId, TestService.ExceptionDescriptor(), string.Empty);
}