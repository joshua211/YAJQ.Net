using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YAJQ.Core;
using YAJQ.Core.Archive;
using YAJQ.Core.JobHandler;
using YAJQ.Core.Persistence;
using YAJQ.Persistence.Memory;
using YAJQ.Tests.Shared;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;

namespace YAJQ.Tests.Integration.Concurrency;

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
        collection.AddSingleton<YAJQOptions>(new YAJQOptions
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