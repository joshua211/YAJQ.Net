using System.Collections.Generic;
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
        await WaitForCompletionAsync(jobId);

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
        await WaitForCompletionAsync(ids.ToList());

        //Assert
        ids.Should().AllSatisfy(i => CallReceiver.WasCalledXTimes(i).Should().BeTrue());
    }

    [Fact]
    public async Task CanHandleJobsWithDelay()
    {
        //Arrange
        var value = "X";
        var id1 = JobId.New.Value;
        var id2 = JobId.New.Value;
        StartJobHandler();

        //Act
        await JobQueue.EnqueueJob(() => Service.DoAsync(value), id1);
        await Task.Delay(500);
        await JobQueue.EnqueueJob(() => Service.DoAsync(value), id2);
        await WaitForCompletionAsync(new List<string> {id1, id2});

        //Assert
        CallReceiver.WasCalledXTimes(value, 2).Should().BeTrue();
    }

    [Fact]
    public async Task CanCompleteSuccessfulJob()
    {
        //Arrange
        var id = "X";
        StartJobHandler();

        //Act
        await JobQueue.EnqueueJob(() => Service.DoAsync(id), id);
        await WaitForCompletionAsync(id);

        //Assert
        var archived = await Persistence.GetCompletedJobsAsync();
        archived.Value.Should().Satisfy(job => job.Id == id && job.State == JobState.Completed);
    }

    [Fact]
    public async Task CanCompleteFailedJob()
    {
        //Arrange
        var id = "X";
        StartJobHandler();

        //Act
        await JobQueue.EnqueueJob(() => Service.DoExceptionAsync(), id);
        await WaitForCompletionAsync(id);

        //Assert
        var archived = await Persistence.GetFailedJobsAsync();
        archived.Value.Should().Satisfy(job => job.Id == id && job.State == JobState.Failed);
    }
}