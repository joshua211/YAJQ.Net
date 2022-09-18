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
        await WaitForCompletionAsync(id, 3000);

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
        await WaitForCompletionAsync(ids, 3000);

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
        var id = (await PublishJobs(1)).First();
        await WaitForCompletionAsync(id);

        CallReceiver.WasCalledXTimes(id).Should().BeTrue();
        cts.Cancel();
    }
}