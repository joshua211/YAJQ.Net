using System.Linq;
using System.Threading.Tasks;
using Fastjob.Tests.Shared;
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
        StartJobHandler(FirstJobHandler);
        StartJobHandler(SecondJobHandler);

        //Act
        var id = (await PublishLongRunningJobs(1)).First();
        await WaitForCompletionAsync(id);

        //Assert
        CallReceiver.WasCalledXTimes(id);
    }

    [Fact]
    public async Task TwoHandlersCanHandleMultipleJobs()
    {
        //Arrange
        StartJobHandler(FirstJobHandler);
        StartJobHandler(SecondJobHandler);

        //Act
        var id = (await PublishLongRunningJobs(1)).First();
        await WaitForCompletionAsync(id);

        //Assert
        CallReceiver.WasCalledXTimes(id);
    }
}