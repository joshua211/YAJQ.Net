using YAJQ.Core.Persistence;
using FluentAssertions;
using Xunit;

namespace YAJQ.Tests.Unit.PersistedJobT;

public class AsapTests
{
    [Fact]
    public void CreatesInstantPersistedJob()
    {
        //Arrange
        var id = JobId.New;
        var descr = TestService.SyncDescriptor();

        //Act
        var persistedJob = PersistedJob.Asap(id, descr);

        //Assert
        persistedJob.Id.Should().Be(id);
        persistedJob.Descriptor.Should().Be(descr);
        persistedJob.State.Should().Be(JobState.Pending);
        persistedJob.JobType.Should().Be(JobType.Instant);
    }
}