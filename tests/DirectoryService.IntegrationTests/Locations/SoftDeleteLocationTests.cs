using System.Net;
using DirectoryService.Infrastructure.Repositories;
using DirectoryService.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace DirectoryService.IntegrationTests.Locations;

public class SoftDeleteLocationTests : DirectoryBaseTests
{
    public SoftDeleteLocationTests(DirectoryTestWebFactory factory) : base(factory) { }

    [Fact]
    public async Task DeleteLocation_SoftDeletes_LeavesRecordInDb()
    {
        // Arrange
        var locationId = await CreateLocationViaHttp();

        // Act
        var response = await Client.DeleteAsync($"/api/locations/{locationId}");

        // Assert HTTP
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // Assert DB — запись осталась, но помечена
        var location = await ExecuteInDb(async db =>
            await db.Locations
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(l => l.Id == locationId));

        Assert.NotNull(location);
        Assert.True(location.IsDeleted);
        Assert.NotNull(location.DeletedWhen);
    }

    [Fact]
    public async Task GetLocation_AfterSoftDelete_Returns404()
    {
        // Arrange
        var locationId = await CreateLocationViaHttp();
        await Client.DeleteAsync($"/api/locations/{locationId}");

        // Act
        var response = await Client.GetAsync($"/api/locations/{locationId}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetLocations_AfterSoftDelete_DoesNotReturnDeleted()
    {
        // Arrange
        var locationId = await CreateLocationViaHttp();
        await Client.DeleteAsync($"/api/locations/{locationId}");

        // Act
        var response = await Client.GetAsync("/api/locations/top");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.DoesNotContain(locationId.ToString(), content);
    }

    [Fact]
    public async Task CleanupService_RemovesOldSoftDeletedRecords()
    {
        // Arrange — создаём и удаляем локацию
        var locationId = await CreateLocationViaHttp();
        await Client.DeleteAsync($"/api/locations/{locationId}");

        // Имитируем "старую" запись — ставим deleted_when в прошлое
        await ExecuteInDb(async db =>
        {
            var location = await db.Locations
                .IgnoreQueryFilters()
                .FirstAsync(l => l.Id == locationId);

            location.SetDeletedWhenForTest(DateTimeOffset.UtcNow.AddDays(-31));
            await db.SaveChangesAsync();
        });

        // Act — дёргаем метод репозитория напрямую, не ждём реального таймера
        await ExecuteInDb(async db =>
        {
            var repo = new LocationRepository(db);
            await repo.DeleteSoftDeletedBatchAsync(
                olderThanUtc: DateTimeOffset.UtcNow,
                batchSize: 100,
                cancellationToken: CancellationToken.None);
        });

        // Assert — запись физически удалена
        var location = await ExecuteInDb(async db =>
            await db.Locations
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(l => l.Id == locationId));

        Assert.Null(location);
    }
}