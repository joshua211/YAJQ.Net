using System.Linq;
using System.Threading.Tasks;
using Fastjob.Core.Persistence;
using Fastjob.Persistence.Memory;
using Fastjob.Tests.Shared;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Abstractions;

namespace Fastjob.Tests.Integration.JobHandler;

public class JobHandlerTests : IntegrationTest
{
    private ITestOutputHelper helper;

    public JobHandlerTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
        this.helper = outputHelper;
    }

    protected override IServiceCollection Configure(IServiceCollection collection)
    {
        collection.AddSingleton<IJobPersistence, MemoryPersistence>();

        return base.Configure(collection);
    }

    [Fact]
    public async Task CanProcessJob()
    {
        //Arrange
        var jobId = JobId.New.Value;
        StartJobHandler();

        //Act
        await JobQueue.EnqueueJob(() => Service.DoAsync(jobId), jobId);
        await Task.Delay(100);

        //Assert
        CallReceiver.WasCalledXTimes(jobId).Should().BeTrue();
    }

    [Fact]
    public async Task CanProcessMultipleJobs()
    {
        //Arrange
        StartJobHandler();

        //Act
        var ids = await AddJobs(5);
        await Task.Delay(100);

        //Assert
        ids.Should().AllSatisfy(i => CallReceiver.WasCalledXTimes(i).Should().BeTrue());
    }

    [Fact]
    public async Task CanHandleJobsWithDelay()
    {
        //Arrange
        var id = "X";
        StartJobHandler();

        //Act
        await JobQueue.EnqueueJob(() => Service.DoAsync(id));
        await Task.Delay(500);
        await JobQueue.EnqueueJob(() => Service.DoAsync(id));
        await Task.Delay(100);

        //Assert
        CallReceiver.WasCalledXTimes(id, 2).Should().BeTrue();
    }
}