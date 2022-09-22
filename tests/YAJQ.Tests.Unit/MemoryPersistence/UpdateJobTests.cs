using System.Threading.Tasks;
using YAJQ.Core.Archive;
using YAJQ.Core.Common;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace YAJQ.Tests.Unit.MemoryPersistence;

public class UpdateJobTests : TestBase
{
    [Fact]
    public async Task UpdatesJobDescriptor()
    {
        //Arrange
        var job = PersistedSyncJob();
        var arch = Substitute.For<IJobArchive>();
        var pers = new Persistence.Memory.MemoryPersistence(arch);
        await pers.SaveJobAsync(job);
        var updated = PersistedAsyncJob();

        //Act
        var update = await pers.UpdateJobAsync(updated);

        //Arrange
        update.WasSuccess.Should().BeTrue();
        var persistedtJob = await pers.GetJobAsync(DefaultJobId);
        persistedtJob.Value.Descriptor.JobName.Should().Be(PersistedAsyncJob().Descriptor.JobName);
    }

    [Fact]
    public async Task UpdatesConcurrencyMark()
    {
        //Arrange
        var arch = Substitute.For<IJobArchive>();
        var job = PersistedSyncJob();
        var pers = new Persistence.Memory.MemoryPersistence(arch);
        await pers.SaveJobAsync(job);
        var conc = "ASDF";
        job.SetToken(conc);

        //Act
        var update = await pers.UpdateJobAsync(job);

        //Arrange
        update.WasSuccess.Should().BeTrue();
        var persistedtJob = await pers.GetJobAsync(DefaultJobId);
        persistedtJob.Value.ConcurrencyToken.Should().Be(conc);
    }

    [Fact]
    public async Task ReturnsNotFoundError()
    {
        //Arrange
        var arch = Substitute.For<IJobArchive>();
        var pers = new Persistence.Memory.MemoryPersistence(arch);
        var updated = PersistedAsyncJob();

        //Act
        var update = await pers.UpdateJobAsync(updated);

        //Arrange
        update.WasSuccess.Should().BeFalse();
        update.Error.Should().Be(Error.NotFound());
    }
}