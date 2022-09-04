using System;
using System.Threading.Tasks;
using Fastjob.Core.JobProcessor;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Fastjob.Tests.Unit.JobProcessor;

public class ProcessJobTests : TestBase
{
    [Fact]
    public async Task CanProcessJobWithoutArguments()
    {
        //Arrange
        var provider = Substitute.For<IServiceProvider>();
        provider.GetService(typeof(TestService)).Returns(service);
        var processor = new DefaultJobProcessor(moduleHelper, provider, Substitute.For<ILogger<DefaultJobProcessor>>(),
            transientFaultHandler);
        var descriptor = PersistedSyncJob().Descriptor;

        //Act
        var result = processor.ProcessJob(descriptor);

        //Assert
        result.WasSuccess.Should().BeTrue();
        service.SyncCalls.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task CanProcessJobWithArguments()
    {
        //Arrange
        var provider = Substitute.For<IServiceProvider>();
        provider.GetService(typeof(TestService)).Returns(service);
        var processor = new DefaultJobProcessor(moduleHelper, provider, Substitute.For<ILogger<DefaultJobProcessor>>(),
            transientFaultHandler);
        var descriptor = TestService.SyncWithValueDescriptor();

        //Act
        var result = processor.ProcessJob(descriptor);

        //Assert
        result.WasSuccess.Should().BeTrue();
        service.SyncCalls.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task CanProcessAsyncJobWithoutArguments()
    {
        //Arrange
        var provider = Substitute.For<IServiceProvider>();
        provider.GetService(typeof(TestService)).Returns(service);
        var processor = new DefaultJobProcessor(moduleHelper, provider, Substitute.For<ILogger<DefaultJobProcessor>>(),
            transientFaultHandler);
        var descriptor = TestService.AsyncDescriptor();

        //Act
        var result = processor.ProcessJob(descriptor);

        //Assert
        result.WasSuccess.Should().BeTrue();
        service.AsyncCalls.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task CanProcessAsyncJobWithArguments()
    {
        //Arrange
        var provider = Substitute.For<IServiceProvider>();
        provider.GetService(typeof(TestService)).Returns(service);
        var processor = new DefaultJobProcessor(moduleHelper, provider, Substitute.For<ILogger<DefaultJobProcessor>>(),
            transientFaultHandler);
        var descriptor = TestService.AsyncWithValueDescriptor();

        //Act
        var result = processor.ProcessJob(descriptor);

        //Assert
        result.WasSuccess.Should().BeTrue();
        service.AsyncCalls.Should().BeGreaterThan(0);
    }
}