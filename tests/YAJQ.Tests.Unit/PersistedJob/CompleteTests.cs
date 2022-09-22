using System;
using FluentAssertions;
using Xunit;
using YAJQ.Core.Persistence;

namespace YAJQ.Tests.Unit.PersistedJobT;

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