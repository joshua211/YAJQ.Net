using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Fastjob.Tests.Shared;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace Fastjob.Tests.Integration.Concurrency;

public class HandlerTests : ConcurrencyTest
{
    public HandlerTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public async Task TwoHandlersCanHandleOneJob()
    {
        //Arrange
        var cts = new CancellationTokenSource();
        StartJobHandler(FirstJobHandler, cts.Token);
        StartJobHandler(SecondJobHandler, cts.Token);

        //Act
        var id = (await PublishLongRunningJobs(1)).First();
        await WaitForCompletionAsync(id);

        //Assert
        CallReceiver.WasCalledXTimes(id).Should().BeTrue();
        cts.Cancel();
    }

    [Fact]
    public async Task TwoHandlersCanHandleMultipleJobs()
    {
        //Arrange
        var cts = new CancellationTokenSource();
        StartJobHandler(FirstJobHandler, cts.Token);
        StartJobHandler(SecondJobHandler, cts.Token);

        //Act
        var ids = (await PublishLongRunningJobs(2));
        await WaitForCompletionAsync(ids, 5000);

        //Assert
        CallReceiver.WasCalledXTimes(ids[0]).Should().BeTrue();
        CallReceiver.WasCalledXTimes(ids[1]).Should().BeTrue();
        cts.Cancel();
    }

    [Fact]
    public async Task TwoHandlersCanHandleJobWhileOneIsBusy()
    {
        //Arrange
        var cts = new CancellationTokenSource();
        StartJobHandler(FirstJobHandler, cts.Token);
        StartJobHandler(SecondJobHandler, cts.Token);

        //Act
        var id1 = await PublishLongRunningJobs(1);
        await Task.Delay(100);
        var id = (await PublishJobs(1)).First();
        await WaitForCompletionAsync(id);

        CallReceiver.WasCalledXTimes(id).Should().BeTrue();
        cts.Cancel();
    }

    [Fact]
    public async Task HandlerCanPickUpQueuedJob()
    {
        //Arrange
        var cts = new CancellationTokenSource();
        var id = (await PublishJobs(1)).First();

        //Act
        StartJobHandler(FirstJobHandler);
        await WaitForCompletionAsync(id);

        //Assert
        CallReceiver.WasCalledXTimes(id);
        cts.Cancel();
    }

    [Fact]
    public async Task AllHandlersCanHandleJobsAtTheSameTime()
    {
        //Arrange
        var cts = new CancellationTokenSource();
        StartJobHandler(FirstJobHandler, cts.Token);
        StartJobHandler(SecondJobHandler, cts.Token);

        //Act
        var ids = await PublishJobs(10);
        await WaitForCompletionAsync(ids.ToList());
        var archived = await Archive.GetArchivedJobsAsync();

        //Assert
        archived.Value.Should().Contain(job => job.HandlerId == FirstJobHandler.HandlerId);
        archived.Value.Should().Contain(job => job.HandlerId == SecondJobHandler.HandlerId);
        cts.Cancel();
    }

    [Fact]
    public async Task HandlerCanCompleteInstantJobThatNeverGotExecuted()
    {
        //Arrange
        var cts = new CancellationTokenSource();
        var faultyHandler = new FaultyJobHandler(Repository, Options);
        StartJobHandler(faultyHandler, cts.Token);
        var id = (await PublishJobs(1)).First();
        await Task.Delay(100);

        //Act
        StartJobHandler(FirstJobHandler, cts.Token);
        await WaitForCompletionAsync(id, 10000);

        //Assert
        CallReceiver.WasCalledXTimes(id).Should().BeTrue();
        cts.Cancel();
    }

    [Fact]
    public async Task HandlerCanCompleteScheduledJobThatNeverGotExecuted()
    {
        //Arrange
        var cts = new CancellationTokenSource();
        var faultyHandler = new FaultyJobHandler(Repository, Options);
        StartJobHandler(faultyHandler, cts.Token);
        var id = (await ScheduleJobs(1, 1)).First();
        await Task.Delay(100);

        //Act
        StartJobHandler(FirstJobHandler, cts.Token);
        await WaitForCompletionAsync(id, 5000);

        //Assert
        CallReceiver.WasCalledXTimes(id).Should().BeTrue();
        cts.Cancel();
    }
}