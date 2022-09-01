using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace Fastjob.Tests.Unit.MemoryPersistence;

public class SaveJobTests : TestBase
{
    [Fact]
    public async Task SaveJobPersistsJob()
    {
        //Arrange
        var job = PersistedSyncJob();
        var pers = new Persistence.Memory.MemoryPersistence();

        //Act
        var result = await pers.SaveJobAsync(job);

        //Assert
        result.WasSuccess.Should().BeTrue();
        var retrievedJob = await pers.GetJobAsync(job.Id);
        retrievedJob.WasSuccess.Should().BeTrue();
        retrievedJob.Value.Id.Should().Be(job.Id);
    }

    [Fact]
    public async Task SaveJobRaisesEvent()
    {
        //Arrange
        var wasRaised = false;
        var job = PersistedSyncJob();
        var pers = new Persistence.Memory.MemoryPersistence();
        pers.NewJob += (s, e) => wasRaised = true;

        //Act
        var result = await pers.SaveJobAsync(job);

        //Assert
        result.WasSuccess.Should().BeTrue();
        wasRaised.Should().BeTrue();
    }

    [Fact]
    public async Task IncreasesMaxCursorByOne()
    {
        //Arrange
        var job = PersistedSyncJob();
        var pers = new Persistence.Memory.MemoryPersistence();
        var initialCursor = await pers.GetCursorAsync();

        //Act
        await pers.SaveJobAsync(job);

        //Assert
        var cursor = await pers.GetCursorAsync();
        cursor.Value.MaxCursor.Should().BeGreaterThan(initialCursor.Value.MaxCursor);
    }
}