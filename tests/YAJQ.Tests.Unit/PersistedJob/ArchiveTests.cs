using System;
using FluentAssertions;
using Xunit;

namespace YAJQ.Tests.Unit.PersistedJobT;

public class ArchiveTests : TestBase
{
    [Fact]
    public void ArchiveShouldCreateArchivedTest()
    {
        //Arrange
        var persistedJob = PersistedSyncJob();
        persistedJob.SetToken("ASDF");
        var handlerId = "HANDLER";
        var processorId = "PROCESSOR";
        var executionTime = TimeSpan.FromSeconds(1);

        //Act
        var archived = persistedJob.Archive(handlerId, processorId, executionTime);

        //Assert
        archived.Descriptor.Should().Be(persistedJob.Descriptor);
        archived.Id.Should().Be(persistedJob.Id);
        archived.State.Should().Be(persistedJob.State);
        archived.ArchiveTime.Second.Should().Be(DateTimeOffset.Now.Second);
        archived.CreationTime.Should().Be(persistedJob.CreationTime);
        archived.ExecutionTime.Should().Be(executionTime);
        archived.HandlerId.Should().Be(handlerId);
        archived.LastUpdated.Second.Should().Be(DateTimeOffset.Now.Second);
        archived.ProcessorId.Should().Be(processorId);
        archived.ScheduledTime.Should().Be(persistedJob.ScheduledTime);
        archived.LastConcurrencyToken.Should().Be(persistedJob.ConcurrencyToken);
    }
}