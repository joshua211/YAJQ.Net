using System.Threading.Tasks;
using Fastjob.Core.Common;
using FluentAssertions;
using Xunit;

namespace Fastjob.Tests.Unit.MemoryPersistence;

public class RemoveAllJobsTests : TestBase
{
    [Fact]
    public async Task RemovesJob()
    {
        //Arrange
        var job1 = PersistedSyncJob();
        var job2 = PersistedSyncJob("ASDF");
        var pers = new Persistence.Memory.MemoryPersistence();
        await pers.SaveJobAsync(job1);
        await pers.SaveJobAsync(job2);

        //Act
        var rm = await pers.RemoveAllJobsAsync();

        //Arrange
        rm.WasSuccess.Should().BeTrue();
        var result1 = await pers.GetJobAsync(JobId);
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
        var pers = new Persistence.Memory.MemoryPersistence();
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