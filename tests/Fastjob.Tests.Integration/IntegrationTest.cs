using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Fastjob.Core;
using Fastjob.Core.Archive;
using Fastjob.Core.JobHandler;
using Fastjob.Core.JobProcessor;
using Fastjob.Core.JobQueue;
using Fastjob.Core.Persistence;
using Fastjob.Core.Utils;
using Fastjob.Tests.Shared;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit.Abstractions;

namespace Fastjob.Tests.Integration;

public abstract class IntegrationTest : IDisposable
{
    private readonly CancellationTokenSource handlerTokenSource;

    public IntegrationTest(ITestOutputHelper outputHelper)
    {
        handlerTokenSource = new CancellationTokenSource();

        IServiceCollection collection = new ServiceCollection();
        collection.AddTransient<IJobQueue, JobQueue>();
        collection.AddTransient<IJobProcessor, DefaultJobProcessor>();
        collection.AddSingleton<IJobRepository, JobRepository>();
        collection.AddTransient<IJobHandler, MultiProcessorJobHandler>();
        collection.AddTransient<IAsyncService, AsyncService>();
        collection.AddTransient<AsyncService, AsyncService>();
        collection.AddTransient<ITransientFaultHandler, DefaultTransientFaultHandler>();
        collection.AddTransient<IJobProcessorFactory, JobProcessorFactory>();
        collection.AddTransient<IProcessorSelectionStrategy, RoundRobinProcessorSelectionStrategy>();
        collection.AddSingleton(Substitute.For<ILogger<MultiProcessorJobHandler>>());
        collection.AddTransient<IModuleHelper, ModuleHelper>();
        collection.AddSingleton<FastjobOptions>(new FastjobOptions
        {
            TransientFaultMaxTries = 1,
        });
        collection.AddLogging();
        collection = Configure(collection);

        Provider = collection.BuildServiceProvider();
        Handler = Provider.GetRequiredService<IJobHandler>();
        JobQueue = Provider.GetRequiredService<IJobQueue>();
        Persistence = Provider.GetRequiredService<IJobPersistence>();
        Service = Provider.GetRequiredService<IAsyncService>();
        Repository = Provider.GetRequiredService<IJobRepository>();
        Archive = Provider.GetRequiredService<IJobArchive>();
    }

    protected IServiceProvider Provider { get; private set; }
    protected IJobHandler Handler { get; private set; }
    protected IJobQueue JobQueue { get; private set; }
    protected IAsyncService Service { get; private set; }
    protected IJobPersistence Persistence { get; private set; }
    protected ILogger Logger { get; private set; }
    protected IJobRepository Repository { get; private set; }
    protected IJobArchive Archive { get; private set; }

    public void Dispose()
    {
        handlerTokenSource.Cancel();
        (Provider as IDisposable)?.Dispose();
    }

    protected virtual IServiceCollection Configure(IServiceCollection collection)
    {
        return collection;
    }

    protected void StartJobHandler(IJobHandler handler = null)
    {
        handler ??= Handler;

        Task.Run(() => Handler.Start(handlerTokenSource.Token), handlerTokenSource.Token);
    }

    protected async Task<IEnumerable<string>> PublishJobs(int amount)
    {
        var ids = new List<string>();
        foreach (var i in Enumerable.Range(0, amount))
        {
            var id = JobId.New;
            await Repository.AddJobAsync(AsyncService.Descriptor(id), id);

            ids.Add(id);
        }

        return ids;
    }

    protected async Task<IEnumerable<string>> ScheduleJobs(int amount)
    {
        var ids = new List<string>();
        foreach (var i in Enumerable.Range(0, amount))
        {
            var id = JobId.New;
            await Repository.AddJobAsync(AsyncService.Descriptor(id), id, DateTimeOffset.Now);

            ids.Add(id);
        }

        return ids;
    }

    public async Task WaitForCompletionAsync(string jobId, int maxWaitTime = 2000) =>
        await WaitForCompletionAsync(new List<string> {jobId}, maxWaitTime);

    public async Task WaitForCompletionAsync(List<string> ids, int maxWaitTime = 2000)
    {
        var tries = 0;
        IEnumerable<string> completedIds;
        do
        {
            await Task.Delay(100);
            tries++;
            completedIds = (await Archive.GetArchivedJobsAsync()).Value.Select(j => j.Id.Value);
            if ((tries * 100) % maxWaitTime == 0)
                break;
        } while (!ids.All(s => completedIds.Contains(s)));
    }
}