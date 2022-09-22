using System;
using System.Threading;
using System.Threading.Tasks;
using YAJQ.Core;
using YAJQ.Core.Common;
using YAJQ.Core.JobHandler;
using YAJQ.Core.Persistence;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace YAJQ.Tests.Unit.JobHandler;

public class HandlerTests : TestBase
{
    [Fact]
    public async Task NewJobWakesUpHandler()
    {
        //Arrange
        var src = new CancellationTokenSource();
        var options = new YAJQOptions
        {
            HandlerTimeout = 100000
        };
        var repository = Substitute.For<IJobRepository>();
        repository.GetNextJobAsync().Returns(Error.NotFound(), PersistedSyncJob());

        var provider = new OpenJobProvider(repository, Substitute.For<ILogger<OpenJobProvider>>(), options);
        var subhandler =
            new ScheduledJobSubHandler(options, repository, Substitute.For<ILogger<ScheduledJobSubHandler>>());

        var handler = new MultiProcessorJobHandler(Substitute.For<ILogger<MultiProcessorJobHandler>>(), moduleHelper,
            repository, options, fakeFactory, new RoundRobinProcessorSelectionStrategy(), provider, subhandler);

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