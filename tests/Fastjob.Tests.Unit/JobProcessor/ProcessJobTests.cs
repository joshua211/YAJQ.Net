using System;
using System.Threading.Tasks;
using Fastjob.Core.JobProcessor;
using FluentAssertions;
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
        var processor = new DefaultJobProcessor(moduleHelper, provider, logger);
        var descriptor = PersistedSyncJob().Descriptor;
        
        //Act
        var result = await processor.ProcessJobAsync(descriptor);
        
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
        var processor = new DefaultJobProcessor(moduleHelper, provider, logger);
        var descriptor = TestService.SyncWithValueDescriptor();
        
        //Act
        var result = await processor.ProcessJobAsync(descriptor);
        
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
        var processor = new DefaultJobProcessor(moduleHelper, provider, logger);
        var descriptor = TestService.AsyncDescriptor();
        
        //Act
        var result = await processor.ProcessJobAsync(descriptor);
        
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
        var processor = new DefaultJobProcessor(moduleHelper, provider, logger);
        var descriptor = TestService.AsyncWithValueDescriptor();
        
        //Act
        var result = await processor.ProcessJobAsync(descriptor);
        
        //Assert
        result.WasSuccess.Should().BeTrue();
        service.AsyncCalls.Should().BeGreaterThan(0);
    }
}