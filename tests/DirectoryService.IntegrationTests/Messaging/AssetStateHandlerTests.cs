using DirectoryService.Application.Messaging.MessageHandlers;
using DirectoryService.Application.ReadModels;
using DirectoryService.Infrastructure;
using DirectoryService.Infrastructure.Repositories;
using IntegrationEvents.Files.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Testcontainers.PostgreSql;

namespace DirectoryService.IntegrationTests.Messaging;

// Отдельная БД: обработчики и SQL проверяются без подключения к рабочему RabbitMQ.
public sealed class AssetStateDatabase : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:18-alpine")
        .WithDatabase("asset_state_tests")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    public DirectoryServiceDbContext CreateContext() => new(
        new DbContextOptionsBuilder<DirectoryServiceDbContext>()
            .UseNpgsql(_postgres.GetConnectionString()).Options);

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();
        await using var db = CreateContext();
        // Как в DirectoryTestWebFactory: создаём текущую модель в пустой тестовой БД.
        await db.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync() => await _postgres.DisposeAsync();
}

public class AssetStateHandlerTests(AssetStateDatabase database) : IClassFixture<AssetStateDatabase>
{
    private static readonly DateTimeOffset Timestamp = new(2026, 10, 5, 10, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task Ready_DeliveredTwice_KeepsOneUnchangedRow()
    {
        var message = NewReady();
        await Ready(message);
        await Ready(message with { OccurredAt = Timestamp.AddMinutes(1) });

        await using var db = database.CreateContext();
        var state = await db.AssetStates.SingleAsync(x => x.AssetId == message.AssetId);
        Assert.Equal(AssetStatus.Ready, state.Status);
        Assert.Equal(message.EntityId, state.EntityId);
        Assert.Equal("location", state.EntityType);
        Assert.Equal("PREVIEW", state.AssetType);
        Assert.Equal(Timestamp, state.OccurredAt);
    }

    [Fact]
    public async Task Deleted_DeliveredTwice_KeepsOneUnchangedRow()
    {
        var ready = NewReady();
        await Ready(ready);
        var deleted = ToDeleted(ready);
        await Deleted(deleted);
        await Deleted(deleted with { OccurredAt = Timestamp.AddHours(1) });

        await using var db = database.CreateContext();
        var state = await db.AssetStates.SingleAsync(x => x.AssetId == ready.AssetId);
        Assert.Equal(AssetStatus.Deleted, state.Status);
        Assert.Equal(deleted.OccurredAt, state.OccurredAt);
    }

    [Fact]
    public async Task DeletedBeforeReady_LateReadyDoesNotRestoreAsset()
    {
        var ready = NewReady();
        await Deleted(ToDeleted(ready));
        await Ready(ready);

        await using var db = database.CreateContext();
        var state = await db.AssetStates.SingleAsync(x => x.AssetId == ready.AssetId);
        Assert.Equal(AssetStatus.Deleted, state.Status);
        Assert.Equal(ToDeleted(ready).OccurredAt, state.OccurredAt);
    }

    [Fact]
    public async Task ConcurrentReadyAndDeleted_ProduceOneDeletedRow()
    {
        var ready = NewReady();
        await Task.WhenAll(Enumerable.Range(0, 10)
            .Select(i => i % 2 == 0 ? Ready(ready) : Deleted(ToDeleted(ready))));

        await using var db = database.CreateContext();
        var state = await db.AssetStates.SingleAsync(x => x.AssetId == ready.AssetId);
        Assert.Equal(AssetStatus.Deleted, state.Status);
    }

    private async Task Ready(AssetReady message)
    {
        await using var db = database.CreateContext();
        await AssetReadyHandler.Handle(message, new AssetStateRepository(db),
            NullLogger<AssetReadyHandler>.Instance, CancellationToken.None);
    }

    private async Task Deleted(AssetDeleted message)
    {
        await using var db = database.CreateContext();
        await AssetDeletedHandler.Handle(message, new AssetStateRepository(db),
            NullLogger<AssetDeletedHandler>.Instance, CancellationToken.None);
    }

    private static AssetReady NewReady() => new(Guid.NewGuid(), Guid.NewGuid(), "location", "PREVIEW", Timestamp);
    private static AssetDeleted ToDeleted(AssetReady ready) => new(ready.AssetId, ready.EntityId,
        ready.EntityType, ready.AssetType, Timestamp.AddMinutes(1));
}
