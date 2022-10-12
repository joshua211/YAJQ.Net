using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Containers;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using Xunit.Abstractions;
using YAJQ.Core.Archive.Interfaces;
using YAJQ.Core.Persistence.Interfaces;
using YAJQ.Persistence.Memory;
using YAJQ.Persistence.Redis;
using YAJQ.Persistence.Redis.JobPersistence;

namespace YAJQ.Tests.Integration.JobHandler;

public class RedisJobHandlerTests : JobHandlerTests
{
    private readonly TestcontainerDatabase container
        = new TestcontainersBuilder<RedisTestcontainer>()
            .WithDatabase(new RedisTestcontainerConfiguration("redis:latest"))
            .Build();

    private IConnectionMultiplexer multiplexer;

    public RedisJobHandlerTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    protected override IServiceCollection Configure(IServiceCollection collection)
    {
        container.StartAsync().Wait();
        multiplexer = ConnectionMultiplexer.Connect(container.ConnectionString);
        collection.AddSingleton<IJobPersistence, RedisPersistence>();
        collection.AddSingleton<IJobArchive, MemoryArchive>();
        collection.AddScoped<IHashSerializer, HashSerializer>();
        collection.AddSingleton(multiplexer);

        return collection;
    }
}