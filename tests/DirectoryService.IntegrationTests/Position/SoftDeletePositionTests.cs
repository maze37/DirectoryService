using System.Net;
using DirectoryService.IntegrationTests.Infrastructure;
using DirectoryService.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DirectoryService.IntegrationTests.Position;

public class SoftDeletePositionTests : DirectoryBaseTests
{
    public SoftDeletePositionTests(DirectoryTestWebFactory factory) : base(factory) { }

    [Fact]
    public async Task DeletePosition_SoftDeletes_LeavesRecordInDb()
    {
        // Arrange
        var departmentId = await CreateDepartmentViaHttp("TEST");
        var positionId = await CreatePositionViaHttp(departmentId: departmentId);

        // Act
        var response = await Client.DeleteAsync($"api/positions/{positionId}");
        
        // Assert HTTP
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        // Assert Db
        var position = await ExecuteInDb(async db =>
            await db.Positions
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.Id == positionId));

        Assert.NotNull(position);
        Assert.True(position.IsDeleted);
        Assert.NotNull(position.DeletedWhen);
    }

    [Fact]
    public async Task GetPosition_AfterSoftDelete_NotVisibleViaDb()
    {
        // Arrange
        var departmentId = await CreateDepartmentViaHttp("TEST");
        var positionId = await CreatePositionViaHttp(departmentId: departmentId);
        await Client.DeleteAsync($"/api/positions/{positionId}");

        // Act — EF с глобальным фильтром (is_deleted = false применяется автоматически)
        var position = await ExecuteInDb(async db =>
            await db.Positions
                .FirstOrDefaultAsync(p => p.Id == positionId));

        // Assert
        Assert.Null(position);
    }

    [Fact]
    public async Task GetPositions_AfterSoftDelete_DoesNotReturnDeleted()
    {
        // Arrange
        var departmentId = await CreateDepartmentViaHttp("TEST");
        var positionId = await CreatePositionViaHttp(departmentId: departmentId);
        await Client.DeleteAsync($"/api/positions/{positionId}");

        // Act
        var positions = await ExecuteInDb(async db =>
            await db.Positions.ToListAsync());

        // Assert
        Assert.DoesNotContain(positions, p => p.Id == positionId);
    }

    [Fact]
    public async Task CleanupService_RemovesOldSoftDeletedPositions()
    {
        // Arrange
        var departmentId = await CreateDepartmentViaHttp("TEST");
        var positionId = await CreatePositionViaHttp(departmentId: departmentId);
        await Client.DeleteAsync($"/api/positions/{positionId}");

        // имитируем "старую" запись
        await ExecuteInDb(async db =>
        {
            var position = await db.Positions
                .IgnoreQueryFilters()
                .FirstAsync(p => p.Id == positionId);

            position.SetDeletedWhenForTest(DateTime.UtcNow.AddDays(-31));
            await db.SaveChangesAsync();
        });

        // Act — вызываем батч-удаление напрямую
        await ExecuteInDb(async db =>
        {
            var repo = new PositionRepository(db);
            await repo.DeleteSoftDeletedBatchAsync(
                olderThanUtc: DateTime.UtcNow,
                batchSize: 100,
                cancellationToken: CancellationToken.None);
        });

        // Assert — запись физически удалена
        var position = await ExecuteInDb(async db =>
            await db.Positions
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(p => p.Id == positionId));

        Assert.Null(position);
    }
}