using FluentAssertions;
using Xunit;

namespace YAJQ.Tests.Unit.PersistedJobT;

public class RefreshTests : TestBase
{
    [Fact]
    public void RefreshUpdatesLastUpdated()
    {
        //Arrange
        var persistedJob = PersistedSyncJob();
        var oldLastUpdate = persistedJob.LastUpdated;

        //Act
        persistedJob.Refresh();

        //Assert
        persistedJob.LastUpdated.Should().NotBe(oldLastUpdate);
    }
}