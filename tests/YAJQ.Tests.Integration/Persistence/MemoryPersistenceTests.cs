using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;
using YAJQ.Core.Archive.Interfaces;
using YAJQ.Core.Persistence;
using YAJQ.Core.Persistence.Interfaces;
using YAJQ.Persistence.Memory;

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