using System;
using System.Threading.Tasks;
using YAJQ.Core.JobQueue;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace YAJQ.Tests.Unit.JobQueue;

public class EnqueueJobTests : TestBase
{
    [Fact]
    public async Task CanEnqueueActionWithoutArguments()
    {
        //Arrange
        var queue = new Core.JobQueue.JobQueue(fakeRepository);

        //Act
        var result = await queue.EnqueueJobAsync(() => service.Something());

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
        var result = await queue.EnqueueJobAsync(() => service.SomethingWithValue(argument));

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
        var result = await queue.EnqueueJobAsync(() => service.SomethingAsync());

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
        var result = await queue.EnqueueJobAsync(() => service.SomethingWithValueAsync(argument));

        //Assert
        result.WasSuccess.Should().BeTrue();
        fakeRepository.Received().AddJobAsync(Arg.Any<JobDescriptor>());
    }

    [Fact]
    public async Task CanEnqueueScheduledActionWithoutArguments()
    {
        //Arrange
        var queue = new Core.JobQueue.JobQueue(fakeRepository);

        //Act
        var result = await queue.ScheduleJobAsync(() => service.Something(), DateTimeOffset.Now);

        //Assert
        result.WasSuccess.Should().BeTrue();
        fakeRepository.Received().AddJobAsync(Arg.Any<JobDescriptor>(), null, Arg.Any<DateTimeOffset>());
    }

    [Fact]
    public async Task CanEnqueueScheduledActionWithArguments()
    {
        //Arrange
        var argument = 1;
        var queue = new Core.JobQueue.JobQueue(fakeRepository);

        //Act
        var result = await queue.ScheduleJobAsync(() => service.SomethingWithValue(argument), DateTimeOffset.Now);

        //Assert
        result.WasSuccess.Should().BeTrue();
        fakeRepository.Received().AddJobAsync(Arg.Any<JobDescriptor>(), null, Arg.Any<DateTimeOffset>());
    }

    [Fact]
    public async Task CanEnqueueScheduledFuncWithoutArguments()
    {
        //Arrange
        var argument = 1;
        var queue = new Core.JobQueue.JobQueue(fakeRepository);

        //Act
        var result = await queue.ScheduleJobAsync(() => service.SomethingAsync(), DateTimeOffset.Now);

        //Assert
        result.WasSuccess.Should().BeTrue();
        fakeRepository.Received().AddJobAsync(Arg.Any<JobDescriptor>(), null, Arg.Any<DateTimeOffset>());
    }

    [Fact]
    public async Task CanEnqueueScheduledFuncWithArguments()
    {
        //Arrange
        var argument = 1;
        var queue = new Core.JobQueue.JobQueue(fakeRepository);

        //Act
        var result = await queue.ScheduleJobAsync(() => service.SomethingWithValueAsync(argument), DateTimeOffset.Now);

        //Assert
        result.WasSuccess.Should().BeTrue();
        fakeRepository.Received().AddJobAsync(Arg.Any<JobDescriptor>(), null, Arg.Any<DateTimeOffset>());
    }
}