using System;
using System.Threading;
using System.Threading.Tasks;
using Fastjob.Core;
using Fastjob.Core.Common;
using Fastjob.Core.JobHandler;
using Fastjob.Core.Persistence;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Fastjob.Tests.Unit.JobHandler;

public class HandlerTests : TestBase
{
    [Fact]
    public async Task NewJobWakesUpHandler()
    {
        //Arrange
        var src = new CancellationTokenSource();
        var options = new FastjobOptions
        {
            HandlerTimeout = 100000
        };
        var repository = Substitute.For<IJobRepository>();
        repository.GetNextJobAsync().Returns(Error.NotFound(), PersistedSyncJob());

        var handler = new MultiProcessorJobHandler(Substitute.For<ILogger<MultiProcessorJobHandler>>(), moduleHelper,
            repository, options, fakeFactory, new RoundRobinProcessorSelectionStrategy());

        //Act
        Task.Run(() => handler.Start(src.Token));
        await Task.Delay(300);
        repository.Update +=
            Raise.Event<EventHandler<JobEvent>>(new object(),
                new JobEvent(Core.Persistence.JobId.New, JobState.Pending));
        await Task.Delay(300);

        //Assert
        repository.Received().TryGetAndMarkJobAsync(Arg.Any<PersistedJob>(), Arg.Any<string>());
        src.Cancel();
    }
}