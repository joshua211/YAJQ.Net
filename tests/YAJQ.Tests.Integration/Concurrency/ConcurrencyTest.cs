using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;
using YAJQ.Core;
using YAJQ.Core.Archive.Interfaces;
using YAJQ.Core.JobHandler;
using YAJQ.Core.JobHandler.Interfaces;
using YAJQ.Core.Persistence;
using YAJQ.Core.Persistence.Interfaces;
using YAJQ.Persistence.Memory;
using YAJQ.Tests.Shared;

namespace YAJQ.Tests.Integration.Concurrency;

public abstract class ConcurrencyTest : IntegrationTest
{
    protected ConcurrencyTest(ITestOutputHelper outputHelper) : base(outputHelper)
    {
        FirstJobHandler = Provider.GetRequiredService<IJobHandler>();
        SecondJobHandler = Provider.GetRequiredService<IJobHandler>();
    }

    public IJobHandler FirstJobHandler { get; }
    public IJobHandler SecondJobHandler { get; }

    protected override IServiceCollection Configure(IServiceCollection collection)
    {
        collection.AddSingleton<IJobPersistence, MemoryPersistence>();
        collection.AddSingleton<IJobArchive, MemoryArchive>();
        collection.AddSingleton(new YAJQOptions
        {
            MaxOverdueTimeout = 5
        });
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