using System.Threading.Tasks;
using Fastjob.Core.Common;
using FluentAssertions;
using Xunit;

namespace Fastjob.Tests.Unit.MemoryPersistence;

public class RemoveJobTests : TestBase
{
    [Fact]
    public async Task RemovesJob()
    {
        //Arrange
        var job = PersistedSyncJob();
        var pers = new Persistence.Memory.MemoryPersistence();
        await pers.SaveJobAsync(job);

        //Act
        var rm = await pers.RemoveJobAsync(JobId);

        //Arrange
        rm.WasSuccess.Should().BeTrue();
        var result = await pers.GetJobAsync(JobId);
        result.WasSuccess.Should().BeFalse();
        result.Error.Should().Be(Error.NotFound());
    }

    [Fact]
    public async Task ReturnsNotFound()
    {
        //Arrange
        var pers = new Persistence.Memory.MemoryPersistence();

        //Act
        var rm = await pers.RemoveJobAsync(JobId);

        //Arrange
        rm.WasSuccess.Should().BeFalse();
        rm.Error.Should().Be(Error.NotFound());
    }

    [Fact]
    public async Task DecreasesMaxCursorByOne()
    {
        //Arrange
        var job = PersistedSyncJob();
        var pers = new Persistence.Memory.MemoryPersistence();
        await pers.SaveJobAsync(job);
        var initialCursor = await pers.GetCursorAsync();

        //Act
        await pers.RemoveJobAsync(JobId);

        //Arrange
        var cursor = await pers.GetCursorAsync();
        cursor.Value.MaxCursor.Should().BeLessThan(initialCursor.Value.MaxCursor);
    }
}