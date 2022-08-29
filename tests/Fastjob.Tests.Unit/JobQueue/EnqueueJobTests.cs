using System.Threading.Tasks;
using Fastjob.Core.JobQueue;
using Fastjob.Core.Persistence;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Fastjob.Tests.Unit.JobQueue;

public class EnqueueJobTests : TestBase
{
    [Fact]
    public async Task CanEnqueueActionWithoutArguments()
    {
        //Arrange
        var queue = new Core.JobQueue.JobQueue(fakeRepository);

        //Act
        var result = await queue.EnqueueJob(() => service.Something());
        
        //Assert
        result.WasSuccess.Should().BeTrue();
        fakeRepository.Received().AddJobAsync(Arg.Any<JobDescriptor>());
    }

    [Fact]
    public async Task CanEnqueueActionWithArguments()
    {
        //Arrange
        var argument = 1;
        var queue = new Core.JobQueue.JobQueue(fakeRepository);

        //Act
        var result = await queue.EnqueueJob(() => service.SomethingWithValue(argument));
        
        //Assert
        result.WasSuccess.Should().BeTrue();
        fakeRepository.Received().AddJobAsync(Arg.Any<JobDescriptor>());
    }

    [Fact]
    public async Task CanEnqueueFuncWithoutArguments()
    {
        //Arrange
        var argument = 1;
        var queue = new Core.JobQueue.JobQueue(fakeRepository);

        //Act
        var result = await queue.EnqueueJob(() => service.SomethingAsync());
        
        //Assert
        result.WasSuccess.Should().BeTrue();
        fakeRepository.Received().AddJobAsync(Arg.Any<JobDescriptor>());
    }
    
    [Fact]
    public async Task CanEnqueueFuncWithArguments()
    {
        //Arrange
        var argument = 1;
        var queue = new Core.JobQueue.JobQueue(fakeRepository);

        //Act
        var result = await queue.EnqueueJob(() => service.SomethingWithValueAsync(argument));
        
        //Assert
        result.WasSuccess.Should().BeTrue();
        fakeRepository.Received().AddJobAsync(Arg.Any<JobDescriptor>());
    }
}