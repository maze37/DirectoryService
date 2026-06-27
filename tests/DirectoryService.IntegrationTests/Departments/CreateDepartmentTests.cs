using System.Net;
using System.Net.Http.Json;
using DirectoryService.Contracts.DepartmentContracts;
using DirectoryService.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace DirectoryService.IntegrationTests.Departments;

public class CreateDepartmentTests : DirectoryBaseTests
{
    public CreateDepartmentTests(DirectoryTestWebFactory factory) : base(factory) { }

    [Fact]
    public async Task CreateDepartment_WithValidData_ShouldSucceed()
    {
        // Arrange
        var locationId = await CreateLocationViaHttp("TEST");
        
        // Act
        var departmentId = await CreateDepartmentViaHttp(slug: "test", locationId: locationId);

        // Assert DB — департамент создался
        var department = await ExecuteInDb(async db =>
            await db.Departments.FirstOrDefaultAsync(d => d.Id == departmentId));

        Assert.NotNull(department);

        // Assert DB — привязан к локации
        var departmentLocation = await ExecuteInDb(async db =>
            await db.DepartmentLocations.FirstOrDefaultAsync(dl =>
                dl.DepartmentId == departmentId &&
                dl.LocationId == locationId));

        Assert.NotNull(departmentLocation);
    }
    
    [Fact]
    public async Task CreateDepartment_WithNonExistingLocation_ShouldFail()
    {
        // Arrange — локация не существует
        var request = new CreateDepartmentRequest(
            Name: "testName",
            Slug: "test",
            ParentId: null,
            LocationIds: [Guid.NewGuid()]);

        // Act
        var response = await Client.PostAsJsonAsync("/api/departments", request);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CreateDepartment_WithEmptyName_ShouldFail()
    {
        // Arrange
        var createdLocationId = await CreateLocationViaHttp("Нукус");

        var request = new CreateDepartmentRequest(
            Name: "",
            Slug: "test",
            ParentId: null,
            LocationIds: [createdLocationId]);

        // Act
        var response = await Client.PostAsJsonAsync("/api/departments", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
    
    [Fact]
    public async Task CreateDepartment_WithDuplicateSlug_Returns409()
    {
        // Arrange — создаём первый отдел со slug "it"
        await CreateDepartmentViaHttp(name: "IT-отдел", slug: "it");
        var locationId = await CreateLocationViaHttp();

        // Act — пытаемся создать второй с тем же slug
        var request = new CreateDepartmentRequest(
            Name: "Другое название",
            Slug: "it",
            ParentId: null,
            LocationIds: [locationId]);

        var response = await Client.PostAsJsonAsync("/api/departments", request);

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }
    
    [Fact]
    public async Task CreateDepartment_ConcurrentDuplicateSlugs_OnlyOneShouldSucceed()
    {
        // Arrange — готовим один и тот же запрос, который два потока отправят одновременно
        var locationId = await CreateLocationViaHttp("Офис");

        var request = new CreateDepartmentRequest(
            Name: "IT-отдел",
            Slug: "test",
            ParentId: null,
            LocationIds: [locationId]);

        // Act — запускаем два одновременных запроса на создание отдела с одним slug
        var task1 = Client.PostAsJsonAsync("/api/departments", request);
        var task2 = Client.PostAsJsonAsync("/api/departments", request);

        var responses = await Task.WhenAll(task1, task2);

        // Assert — один должен успешно создаться, второй должен получить 409
        var statusCodes = responses.Select(r => r.StatusCode).ToList();
        
        Assert.Contains(HttpStatusCode.OK, statusCodes);    // один успех
        Assert.Contains(HttpStatusCode.Conflict, statusCodes);   // один конфликт

        // Assert DB — в базе ровно один отдел с этим slug
        var count = await ExecuteInDb(async db =>
            await db.Departments.CountAsync(d => d.Slug == "test"));
        Assert.Equal(1, count);
    }
}