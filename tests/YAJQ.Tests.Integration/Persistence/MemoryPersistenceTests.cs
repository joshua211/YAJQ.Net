using YAJQ.Core.Archive;
using YAJQ.Core.Persistence;
using YAJQ.Persistence.Memory;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;

namespace YAJQ.Tests.Integration.Persistence;

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