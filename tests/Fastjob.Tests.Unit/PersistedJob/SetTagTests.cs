using FluentAssertions;
using Xunit;

namespace Fastjob.Tests.Unit.PersistedJobT;

public class SetTagTests : TestBase
{
    [Fact]
    public void SetsTheCorrectToken()
    {
        //Arrange
        var token = "ASDF";
        var persistedJob = PersistedSyncJob();

        //Act
        persistedJob.SetToken(token);

        //Asser
        persistedJob.ConcurrencyToken.Should().Be(token);
    }
}