using System;
using YAJQ.Core.Persistence;
using FluentAssertions;
using Xunit;

namespace YAJQ.Tests.Unit.PersistedJobT;

public class ScheduledTests : TestBase
{
    [Fact]
    public void CreatesScheduledPersistedJob()
    {
        //Arrange
        var id = JobId.New;
        var descr = TestService.SyncDescriptor();
        var scheduledTime = DateTimeOffset.Now;

        //Act
        var persistedJob = PersistedJob.Scheduled(id, descr, scheduledTime);

        //Assert
        persistedJob.Id.Should().Be(id);
        persistedJob.Descriptor.Should().Be(descr);
        persistedJob.State.Should().Be(JobState.Pending);
        persistedJob.ScheduledTime.Should().Be(scheduledTime);
        persistedJob.JobType.Should().Be(JobType.Scheduled);
    }
}