using Fastjob.Core;
using Fastjob.Core.JobProcessor;
using Fastjob.Core.Persistence;
using Fastjob.Core.Utils;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Fastjob.Tests.Unit;

public abstract class TestBase
{
    protected IJobPersistence fakePersistence;
    protected IJobRepository fakeRepository;
    protected ILogger logger;
    protected IModuleHelper moduleHelper;
    protected FastjobOptions options;
    protected TestService service;
    protected ITransientFaultHandler transientFaultHandler;

    public TestBase()
    {
        fakePersistence = Substitute.For<IJobPersistence>();
        fakeRepository = Substitute.For<IJobRepository>();
        moduleHelper = new ModuleHelper();
        logger = Substitute.For<ILogger>();
        service = new TestService();
        options = new FastjobOptions();
        transientFaultHandler =
            new DefaultTransientFaultHandler(Substitute.For<ILogger<DefaultTransientFaultHandler>>(), options);
    }

    public string JobId => "XXXXXXXXXXXXXXXXX";

    public PersistedJob PersistedSyncJob(string? id = null) =>
        new(Core.Persistence.JobId.With(id ?? JobId), TestService.SyncDescriptor(), string.Empty);

    public PersistedJob PersistedAsyncJob(string? id = null) =>
        new(Core.Persistence.JobId.With(id ?? JobId), TestService.AsyncDescriptor(), string.Empty);

    public PersistedJob PersistedExcJob(string? id = null) =>
        new(Core.Persistence.JobId.With(id ?? JobId), TestService.ExceptionDescriptor(), string.Empty);
}