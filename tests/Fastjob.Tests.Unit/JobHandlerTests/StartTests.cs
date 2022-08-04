using System;
using System.Threading;
using System.Threading.Tasks;
using Castle.Core.Logging;
using Fastjob.Core.Interfaces;
using Fastjob.Core.JobHandler;
using Fastjob.Core.Utils;
using Fastjob.Persistence.Memory;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Fastjob.Tests.Unit.JobHandlerTests;

public class StartTests
{
    private readonly TestService testService;
    private readonly IJobHandler handler;
    private readonly IJobStorage storage;
    
    public StartTests()
    {
        storage = new MemoryJobStorage();
        testService = new TestService();
        var logger = Substitute.For<ILogger<DefaultJobHandler>>();

        var provider = Substitute.For<IServiceProvider>();
        provider.GetService(typeof(TestService)).Returns(testService);

        var moduleHelper = Substitute.For<IModuleHelper>();
        moduleHelper.IsModuleLoaded("Fastjob.Tests.Unit.dll").Returns(true);

        handler = new DefaultJobHandler(logger, provider, moduleHelper, storage);
    }

    [Fact]
    public async Task HandlerShouldExecuteAddedJob()
    {
        var tokenSource = new CancellationTokenSource();
        Task.Run(() => handler.Start(tokenSource.Token));
        
        await storage.AddJobAsync(testService.SyncDescriptor());
        await Task.Delay(1100);

        TestService.SyncCalls.Should().NotBe(0);
        tokenSource.Cancel();
    }

    [Fact]
    public async Task HandlerShouldExecuteAllAddedJobs()
    {
        var tokenSource = new CancellationTokenSource();
        Task.Run(() => handler.Start(tokenSource.Token));
        
        await storage.AddJobAsync(testService.SyncDescriptor());
        await storage.AddJobAsync(testService.SyncDescriptor());
        await storage.AddJobAsync(testService.SyncDescriptor());
        await Task.Delay(1100);
        
        TestService.SyncCalls.Should().BeGreaterOrEqualTo(3);
        tokenSource.Cancel();
    }
    
    [Fact]
    public async Task HandlerShouldExecute()
    {
        var tokenSource = new CancellationTokenSource();
        Task.Run(() => handler.Start(tokenSource.Token));
        
        await storage.AddJobAsync(testService.SyncDescriptor());
        await storage.AddJobAsync(testService.SyncDescriptor());
        await storage.AddJobAsync(testService.SyncDescriptor());
        await Task.Delay(1100);
        
        TestService.SyncCalls.Should().BeGreaterOrEqualTo(3);
        tokenSource.Cancel();
    }
}