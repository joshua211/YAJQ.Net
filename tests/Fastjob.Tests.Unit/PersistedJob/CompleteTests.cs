using System;
using Fastjob.Core.Persistence;
using FluentAssertions;
using Xunit;

namespace Fastjob.Tests.Unit.PersistedJobT;

public class CompleteTests : TestBase
{
    [Fact]
    public void CompleteSetsStateToCompleted()
    {
        //Arrange
        var persistedJob = PersistedSyncJob();
        var oldState = persistedJob.State;

        //Act
        persistedJob.Complete();

        //Assert
        persistedJob.State.Should().NotBe(oldState);
        persistedJob.State.Should().Be(JobState.Completed);
    }

    [Fact]
    public void CantCompleteJobThatIsNotPending()
    {
        //Arrange
        var persistedJob = PersistedSyncJob();
        persistedJob.Fail();

        //Act
        var act = () => persistedJob.Complete();

        //Assert
        act.Should().Throw<Exception>();
    }
}