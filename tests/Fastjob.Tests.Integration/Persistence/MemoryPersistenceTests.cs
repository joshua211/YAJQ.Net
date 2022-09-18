using Fastjob.Core.Archive;
using Fastjob.Core.Persistence;
using Fastjob.Persistence.Memory;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;

namespace Fastjob.Tests.Integration.Persistence;

public class MemoryPersistenceTests : PersistenceTests
{
    public MemoryPersistenceTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    protected override IServiceCollection Configure(IServiceCollection collection)
    {
        collection.AddSingleton<IJobPersistence, MemoryPersistence>();
        collection.AddSingleton<IJobArchive, MemoryArchive>();
        return base.Configure(collection);
    }
}