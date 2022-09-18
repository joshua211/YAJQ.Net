using System;
using Fastjob.Core.Persistence;
using FluentAssertions;
using Xunit;

namespace Fastjob.Tests.Unit.PersistedJobT;

public class FailTests : TestBase
{
    [Fact]
    public void FailSetsStateToFailed()
    {
        //Arrange
        var persistedJob = PersistedSyncJob();
        var oldState = persistedJob.State;

        //Act
        persistedJob.Fail();

        //Assert
        persistedJob.State.Should().NotBe(oldState);
        persistedJob.State.Should().Be(JobState.Failed);
    }

    [Fact]
    public void CantFailJobThatIsNotPending()
    {
        //Arrange
        var persistedJob = PersistedSyncJob();
        persistedJob.Complete();

        //Act
        var act = () => persistedJob.Fail();

        //Assert
        act.Should().Throw<Exception>();
    }
}