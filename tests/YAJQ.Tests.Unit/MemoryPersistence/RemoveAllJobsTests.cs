using System.Threading.Tasks;
using YAJQ.Core.Archive;
using YAJQ.Core.Common;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace YAJQ.Tests.Unit.MemoryPersistence;

public class RemoveAllJobsTests : TestBase
{
    [Fact]
    public async Task RemovesJob()
    {
        //Arrange
        var job1 = PersistedSyncJob();
        var job2 = PersistedSyncJob("ASDF");
        var arch = Substitute.For<IJobArchive>();
        var pers = new Persistence.Memory.MemoryPersistence(arch);
        await pers.SaveJobAsync(job1);
        await pers.SaveJobAsync(job2);

        //Act
        var rm = await pers.RemoveAllJobsAsync();

        //Arrange
        rm.WasSuccess.Should().BeTrue();
        var result1 = await pers.GetJobAsync(DefaultJobId);
        result1.WasSuccess.Should().BeFalse();
        result1.Error.Should().Be(Error.NotFound());

        var result2 = await pers.GetJobAsync("ASDF");
        result2.WasSuccess.Should().BeFalse();
        result2.Error.Should().Be(Error.NotFound());
    }

    [Fact]
    public async Task ResetsCursorToZero()
    {
        //Arrange
        var job1 = PersistedSyncJob();
        var job2 = PersistedSyncJob("ASDF");
        var arch = Substitute.For<IJobArchive>();
        var pers = new Persistence.Memory.MemoryPersistence(arch);
        await pers.SaveJobAsync(job1);
        await pers.SaveJobAsync(job2);

        //Act
        var rm = await pers.RemoveAllJobsAsync();

        //Arrange
        var cursor = await pers.GetCursorAsync();
        cursor.WasSuccess.Should().BeTrue();
        cursor.Value.CurrentCursor.Should().Be(0);
        cursor.Value.MaxCursor.Should().Be(0);
    }
}