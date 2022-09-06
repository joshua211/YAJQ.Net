using System;
using Fastjob.Core;
using Fastjob.Core.JobProcessor;
using Fastjob.Core.Persistence;
using Fastjob.Core.Utils;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Fastjob.Tests.Unit;

public abstract class TestBase
{
    protected IJobProcessorFactory fakeFactory;
    protected IJobPersistence fakePersistence;
    protected IServiceProvider fakeProvider;
    protected IJobRepository fakeRepository;
    protected ILogger logger;
    protected IModuleHelper moduleHelper;
    protected FastjobOptions options;
    protected TestService service;
    protected ITransientFaultHandler transientFaultHandler;

    public TestBase()
    {
        fakeProvider = Substitute.For<IServiceProvider>();
        fakeProvider.GetService(typeof(TestService)).Returns(service);
        fakePersistence = Substitute.For<IJobPersistence>();
        fakeRepository = Substitute.For<IJobRepository>();
        fakeRepository.AddJobAsync(default, default, default).ReturnsForAnyArgs(JobId);
        moduleHelper = new ModuleHelper();
        logger = Substitute.For<ILogger>();
        service = new TestService();
        options = new FastjobOptions();
        transientFaultHandler =
            new DefaultTransientFaultHandler(Substitute.For<ILogger<DefaultTransientFaultHandler>>(), options);
        fakeFactory = Substitute.For<IJobProcessorFactory>();
        fakeFactory.New().Returns(new DefaultJobProcessor(moduleHelper, fakeProvider,
            Substitute.For<ILogger<DefaultJobProcessor>>(), transientFaultHandler));
    }

    public string JobId => "XXXXXXXXXXXXXXXXX";

    public PersistedJob PersistedSyncJob(string? id = null) =>
        PersistedJob.Asap(Core.Persistence.JobId.With(id ?? JobId), TestService.SyncDescriptor());

    public PersistedJob PersistedAsyncJob(string? id = null) =>
        PersistedJob.Asap(Core.Persistence.JobId.With(id ?? JobId), TestService.AsyncDescriptor());

    public PersistedJob PersistedExcJob(string? id = null) =>
        PersistedJob.Asap(Core.Persistence.JobId.With(id ?? JobId), TestService.ExceptionDescriptor());
}