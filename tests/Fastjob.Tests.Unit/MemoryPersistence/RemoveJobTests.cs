using System.Threading.Tasks;
using Fastjob.Core.Archive;
using Fastjob.Core.Common;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Fastjob.Tests.Unit.MemoryPersistence;

public class RemoveJobTests : TestBase
{
    [Fact]
    public async Task RemovesJob()
    {
        //Arrange
        var job = PersistedSyncJob();
        var arch = Substitute.For<IJobArchive>();
        var pers = new Persistence.Memory.MemoryPersistence(arch);
        await pers.SaveJobAsync(job);

        //Act
        var rm = await pers.RemoveJobAsync(DefaultJobId);

        //Arrange
        rm.WasSuccess.Should().BeTrue();
        var result = await pers.GetJobAsync(DefaultJobId);
        result.WasSuccess.Should().BeFalse();
        result.Error.Should().Be(Error.NotFound());
    }

    [Fact]
    public async Task ReturnsNotFound()
    {
        //Arrange
        var arch = Substitute.For<IJobArchive>();
        var pers = new Persistence.Memory.MemoryPersistence(arch);

        //Act
        var rm = await pers.RemoveJobAsync(DefaultJobId);

        //Arrange
        rm.WasSuccess.Should().BeFalse();
        rm.Error.Should().Be(Error.NotFound());
    }

    [Fact]
    public async Task DecreasesMaxCursorByOne()
    {
        //Arrange
        var job = PersistedSyncJob();
        var arch = Substitute.For<IJobArchive>();
        var pers = new Persistence.Memory.MemoryPersistence(arch);
        await pers.SaveJobAsync(job);
        var initialCursor = await pers.GetCursorAsync();

        //Act
        await pers.RemoveJobAsync(DefaultJobId);

        //Arrange
        var cursor = await pers.GetCursorAsync();
        cursor.Value.MaxCursor.Should().BeLessThan(initialCursor.Value.MaxCursor);
    }
}