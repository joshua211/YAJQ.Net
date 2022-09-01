using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Fastjob.Core;
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
        collection.AddTransient<IJobRepository, JobRepository>();
        collection.AddTransient<IJobHandler, DefaultJobHandler>();
        collection.AddTransient<IAsyncService, AsyncService>();
        collection.AddTransient<AsyncService, AsyncService>();
        collection.AddLogging();
        collection.AddSingleton(Substitute.For<ILogger<DefaultJobHandler>>());
        collection.AddTransient<IModuleHelper, ModuleHelper>();
        collection.AddTransient<FastjobOptions>();
        collection = Configure(collection);

        Provider = collection.BuildServiceProvider();
        Handler = Provider.GetRequiredService<IJobHandler>();
        JobQueue = Provider.GetRequiredService<IJobQueue>();
        Persistence = Provider.GetRequiredService<IJobPersistence>();
        Service = Provider.GetRequiredService<IAsyncService>();
        Repository = Provider.GetRequiredService<IJobRepository>();
    }

    protected IServiceProvider Provider { get; private set; }
    protected IJobHandler Handler { get; private set; }
    protected IJobQueue JobQueue { get; private set; }
    protected IAsyncService Service { get; private set; }
    protected IJobPersistence Persistence { get; private set; }
    protected ILogger Logger { get; private set; }
    protected IJobRepository Repository { get; private set; }

    public void Dispose()
    {
        handlerTokenSource.Dispose();
    }

    protected virtual IServiceCollection Configure(IServiceCollection collection)
    {
        return collection;
    }

    protected void StartJobHandler()
    {
        Task.Run(() => Handler.Start(handlerTokenSource.Token));
    }

    protected async Task<IEnumerable<string>> AddJobs(int amount)
    {
        var ids = new List<string>();
        foreach (var i in Enumerable.Range(0, amount))
        {
            var id = i.ToString();
            await Repository.AddJobAsync(AsyncService.Descriptor(id), id);

            ids.Add(id);
        }

        return ids;
    }
}