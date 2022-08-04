using System;
using System.Reflection;
using System.Threading.Tasks;
using Fastjob.Core.Common;
using Fastjob.Core.Interfaces;
using Fastjob.Core.JobProcessor;
using Fastjob.Core.JobQueue;
using Fastjob.Core.Utils;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Fastjob.Tests.Unit.JobProcessorTests;

public class ProcessJobTests
{
    private readonly IJobStorage storage;
    private readonly TestService testService;
    private readonly IJobProcessor processor;

    public ProcessJobTests()
    {
        testService = Substitute.For<TestService>();
        storage = Substitute.For<IJobStorage>();
        var logger = Substitute.For<ILogger>();

        var provider = Substitute.For<IServiceProvider>();
        provider.GetService(typeof(TestService)).Returns(testService);

        var moduleHelper = Substitute.For<IModuleHelper>();
        moduleHelper.IsModuleLoaded("Fastjob.Tests.Unit.dll").Returns(true);
        processor = new DefaultJobProcessor(moduleHelper, provider, logger);
    }

    [Fact]
    public void DefaultProcessorIdShouldNotBeEmpty()
    {
        var id = processor.ProcessorId;

        id.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task ProcessorShouldExecuteTestServiceWithSyncDescriptor()
    {
        var arg = 1;
        var descriptor = new TestService().SyncDescriptor();

        var result = await processor.ProcessJobAsync(descriptor);

        result.WasSuccess.Should().BeTrue();
        testService.Received().Something(arg);
    }
    
    [Fact]
    public async Task ProcessorShouldExecuteTestServiceWithAsyncDescriptor()
    {
        var arg = 1;
        var descriptor = new TestService().AsyncDescriptor();

        var result = await processor.ProcessJobAsync(descriptor);

        result.WasSuccess.Should().BeTrue();
        testService.Received().SomethingAsync(arg);
    }

    [Fact]
    public async Task ProcessorShouldReturnModuleErrorIfModuleIsNotLoaded()
    {
        var correctDescriptor = new TestService().SyncDescriptor();
        var faultedDescriptor = new JobDescriptor(correctDescriptor.JobName, correctDescriptor.FullTypeName,
            "FAULTED MODULE NAME", correctDescriptor.Args);

        var result = await processor.ProcessJobAsync(faultedDescriptor);

        result.WasSuccess.Should().BeFalse();
        result.Error.Should().Be(Error.ModuleNotLoaded());
    }
    
    [Fact]
    public async Task ProcessorShouldReturnTypeErrorIfTypeIsNotFound()
    {
        var correctDescriptor = new TestService().SyncDescriptor();
        var faultedDescriptor = new JobDescriptor(correctDescriptor.JobName, "Faulted type",
            correctDescriptor.ModuleName, correctDescriptor.Args);

        var result = await processor.ProcessJobAsync(faultedDescriptor);

        result.WasSuccess.Should().BeFalse();
        result.Error.Should().Be(Error.TypeNotFound());
    }
    
    [Fact]
    public async Task ProcessorShouldReturnMethodBaseErrorIfMethodBaseWasNotConstructed()
    {
        var correctDescriptor = new TestService().SyncDescriptor();
        var faultedDescriptor = new JobDescriptor("Faulted", correctDescriptor.FullTypeName,
            correctDescriptor.ModuleName, correctDescriptor.Args);

        var result = await processor.ProcessJobAsync(faultedDescriptor);

        result.WasSuccess.Should().BeFalse();
        result.Error.Should().Be(Error.MethodBaseNotFound());
    }

    [Fact]
    public async Task ProcessorShouldReturnFailedExecutionOnException()
    {
        var descriptor = new TestService().ExceptionDescriptor();

        var result = await processor.ProcessJobAsync(descriptor);

        result.WasSuccess.Should().BeFalse();
        result.Error.Should().Be(Error.ExecutionFailed());
    }
}