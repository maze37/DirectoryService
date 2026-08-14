using System.Net;
using DirectoryService.IntegrationTests.Infrastructure;
using DirectoryService.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Renci.SshNet.Sftp;

namespace DirectoryService.IntegrationTests.Departments;

public class SoftDeleteDepartmentTests : DirectoryBaseTests
{
    public SoftDeleteDepartmentTests(DirectoryTestWebFactory factory) : base(factory) { }

    [Fact]
    public async Task DeleteDepartment_SoftDeletes_LeavesRecordInDb()
    {
        // Arrange
        var departmentId = await CreateDepartmentViaHttp(name: "TEST", slug: "test");

        // Act
        var response = await Client.DeleteAsync($"/api/departments/{departmentId}");

        // Assert HTTP
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // Assert DB — запись осталась, но помечена
        var department = await ExecuteInDb(async db =>
            await db.Departments
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(d => d.Id == departmentId));

        Assert.NotNull(department);
        Assert.True(department.IsDeleted);
        Assert.NotNull(department.DeletedWhen);
    }

    [Fact]
    public async Task GetDepartment_AfterSoftDelete_Returns404()
    {
        // Arrange
        var departmentId = await CreateDepartmentViaHttp(name: "TEST", slug: "test");
        await Client.DeleteAsync($"/api/departments/{departmentId}");

        // Act
        var response = await Client.GetAsync($"/api/departments/{departmentId}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetDepartments_AfterSoftDelete_DoesNotReturnDeleted()
    {
        // Arrange
        var departmentId = await CreateDepartmentViaHttp(name: "TEST", slug: "test");
        await Client.DeleteAsync($"/api/departments/{departmentId}");

        // Act
        var response = await Client.GetAsync("/api/departments");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.DoesNotContain(departmentId.ToString(), content);
    }

    [Fact]
    public async Task CleanupService_RemovesOldSoftDeletedDepartments()
    {
        // Arrange
        var departmentId = await CreateDepartmentViaHttp(name: "TEST", slug: "test");
        await Client.DeleteAsync($"/api/departments/{departmentId}");

        // делаем запись "старой"
        await ExecuteInDb(async db =>
        {
            var department = await db.Departments
                .IgnoreQueryFilters()
                .FirstAsync(d => d.Id == departmentId);

            department.SetDeletedWhenForTest(DateTime.UtcNow.AddDays(-31));
            await db.SaveChangesAsync();
        });

        // Act — вызываем батч-удаление напрямую, не дожидаясь таймера
        await ExecuteInDb(async db =>
        {
            var repo = new DepartmentRepository(db);
            await repo.DeleteSoftDeletedBatchAsync(
                olderThanUtc: DateTime.UtcNow,
                batchSize: 100,
                cancellationToken: CancellationToken.None);
        });

        // Assert — запись физически удалена
        var department = await ExecuteInDb(async db =>
            await db.Departments
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(d => d.Id == departmentId));

        Assert.Null(department);
    }

    [Fact]
    public async Task RestoreSoftDeletedDepartment_ShouldRestoreIt()
    {
        // Arrange
        var departmentId = await CreateDepartmentViaHttp(slug: "OFFICE");
        await Client.DeleteAsync($"/api/departments/{departmentId}");
        
        // Act
        var response = await Client.PutAsync($"/api/departments/{departmentId}/restore", null);

        // Assert HTTP
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        // Assert DB
        var department = await ExecuteInDb(async db =>
            await db.Departments
                .FirstOrDefaultAsync(d => d.Id == departmentId));
        
        Assert.NotNull(department);
        Assert.False(department.IsDeleted);
        Assert.Null(department.DeletedWhen);
    }
}