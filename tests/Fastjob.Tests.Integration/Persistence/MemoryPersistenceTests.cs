using Fastjob.Core.Persistence;
using Fastjob.Persistence.Memory;
using Microsoft.Extensions.DependencyInjection;

namespace Fastjob.Tests.Integration.Persistence;

public class MemoryPersistenceTests : PersistenceTests
{
    protected override IServiceCollection Configure(IServiceCollection collection)
    {
        collection.AddSingleton<IJobPersistence>(new MemoryPersistence());
        return base.Configure(collection);
    }
}