﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;
using YAJQ.Core.Persistence;
using YAJQ.Tests.Shared;

namespace YAJQ.Tests.Integration.JobHandler;

public abstract class JobHandlerTests : IntegrationTest
{
    private ITestOutputHelper helper;

    public JobHandlerTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
        helper = outputHelper;
        StartJobHandler();
    }

    [Fact]
    public async Task CanProcessJob()
    {
        //Arrange
        var jobId = JobId.New.Value;

        //Act
        await JobQueue.EnqueueJobAsync(() => Service.DoAsync(jobId), jobId);
        await WaitForCompletionAsync(jobId);

        //Assert
        CallReceiver.WasCalledXTimes(jobId).Should().BeTrue();
    }

    [Fact]
    public async Task CanProcessMultipleJobs()
    {
        //Arrange

        //Act
        var ids = await PublishJobs(12);
        await WaitForCompletionAsync(ids.ToList());

        //Assert
        ids.Should().AllSatisfy(i => CallReceiver.WasCalledXTimes(i).Should().BeTrue());
    }

    [Fact]
    public async Task CanHandleJobsWithDelay()
    {
        //Arrange
        var value = JobId.New;
        var id1 = JobId.New.Value;
        var id2 = JobId.New.Value;

        //Act
        await JobQueue.EnqueueJobAsync(() => Service.DoAsync(value), id1);
        await Task.Delay(500);
        await JobQueue.EnqueueJobAsync(() => Service.DoAsync(value), id2);
        await WaitForCompletionAsync(new List<string> {id1, id2});

        //Assert
        CallReceiver.WasCalledXTimes(value, 2).Should().BeTrue();
    }

    [Fact]
    public async Task CanCompleteSuccessfulJob()
    {
        //Arrange
        var id = JobId.New;

        //Act
        await JobQueue.EnqueueJobAsync(() => Service.DoAsync(id), id);
        await WaitForCompletionAsync(id);

        //Assert
        var archived = await Archive.GetCompletedJobsAsync();
        archived.Value.Should().Satisfy(job => job.Id == id.Value && job.State == JobState.Completed);
    }

    [Fact]
    public async Task CanCompleteFailedJob()
    {
        //Arrange
        var id = JobId.New;

        //Act
        await JobQueue.EnqueueJobAsync(() => Service.DoExceptionAsync(), id);
        await WaitForCompletionAsync(id);

        //Assert
        var archived = await Archive.GetFailedJobsAsync();
        archived.Value.Should().Satisfy(job => job.Id == id.Value && job.State == JobState.Failed);
    }

    [Fact]
    public async Task CanProcessScheduledJob()
    {
        //Arrange
        var jobId = JobId.New.Value;

        //Act
        await JobQueue.ScheduleJobAsync(() => Service.DoAsync(jobId), DateTimeOffset.Now, jobId);
        await WaitForCompletionAsync(jobId);

        //Assert
        CallReceiver.WasCalledXTimes(jobId).Should().BeTrue();
    }

    [Fact]
    public async Task CanProcessMultipleScheduledJobs()
    {
        //Arrange

        //Act
        var ids = await ScheduleJobs(12);
        await WaitForCompletionAsync(ids.ToList());

        //Assert
        ids.Should().AllSatisfy(i => CallReceiver.WasCalledXTimes(i).Should().BeTrue());
    }

    [Fact]
    public async Task CanProcessScheduledJobInFuture()
    {
        //Arrange
        var jobId = JobId.New.Value;
        var now = DateTimeOffset.Now;
        var scheduledTime = DateTimeOffset.Now.AddSeconds(5);

        //Act
        await JobQueue.ScheduleJobAsync(() => Service.DoAsync(jobId), scheduledTime, jobId);
        await WaitForCompletionAsync(jobId);

        //Assert
        CallReceiver.WasCalledAfter(jobId, now).Should().BeTrue();
    }
}