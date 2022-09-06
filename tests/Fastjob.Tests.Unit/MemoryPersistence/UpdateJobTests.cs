using System.Threading.Tasks;
using Fastjob.Core.Common;
using FluentAssertions;
using Xunit;

namespace Fastjob.Tests.Unit.MemoryPersistence;

public class UpdateJobTests : TestBase
{
    [Fact]
    public async Task UpdatesJobDescriptor()
    {
        //Arrange
        var job = PersistedSyncJob();
        var pers = new Persistence.Memory.MemoryPersistence();
        await pers.SaveJobAsync(job);
        var updated = PersistedAsyncJob();

        //Act
        var update = await pers.UpdateJobAsync(updated);

        //Arrange
        update.WasSuccess.Should().BeTrue();
        var persistedtJob = await pers.GetJobAsync(JobId);
        persistedtJob.Value.Descriptor.JobName.Should().Be(PersistedAsyncJob().Descriptor.JobName);
    }

    [Fact]
    public async Task UpdatesConcurrencyMark()
    {
        //Arrange
        var job = PersistedSyncJob();
        var pers = new Persistence.Memory.MemoryPersistence();
        await pers.SaveJobAsync(job);
        var conc = "ASDF";
        job.SetTag(conc);

        //Act
        var update = await pers.UpdateJobAsync(job);

        //Arrange
        update.WasSuccess.Should().BeTrue();
        var persistedtJob = await pers.GetJobAsync(JobId);
        persistedtJob.Value.ConcurrencyTag.Should().Be(conc);
    }

    [Fact]
    public async Task ReturnsNotFoundError()
    {
        //Arrange
        var pers = new Persistence.Memory.MemoryPersistence();
        var updated = PersistedAsyncJob();

        //Act
        var update = await pers.UpdateJobAsync(updated);

        //Arrange
        update.WasSuccess.Should().BeFalse();
        update.Error.Should().Be(Error.NotFound());
    }
}