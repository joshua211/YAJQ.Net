using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fastjob.Core.Archive;
using Fastjob.Core.JobHandler;
using Fastjob.Core.Persistence;
using Fastjob.Persistence.Memory;
using Fastjob.Tests.Shared;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;

namespace Fastjob.Tests.Integration.Concurrency;

public abstract class ConcurrencyTest : IntegrationTest
{
    protected ConcurrencyTest(ITestOutputHelper outputHelper) : base(outputHelper)
    {
        FirstJobHandler = Provider.GetRequiredService<IJobHandler>();
        SecondJobHandler = Provider.GetRequiredService<IJobHandler>();
    }

    public IJobHandler FirstJobHandler { get; private set; }
    public IJobHandler SecondJobHandler { get; private set; }

    protected override IServiceCollection Configure(IServiceCollection collection)
    {
        collection.AddSingleton<IJobPersistence, MemoryPersistence>();
        collection.AddSingleton<IJobArchive, MemoryArchive>();
        return base.Configure(collection);
    }

    public async Task<List<string>> PublishLongRunningJobs(int amount)
    {
        var ids = new List<string>();
        foreach (var i in Enumerable.Range(0, amount))
        {
            var id = JobId.New;
            await Repository.AddJobAsync(AsyncService.LongRunningDescriptor(id), id);

            ids.Add(id);
        }

        return ids;
    }
}