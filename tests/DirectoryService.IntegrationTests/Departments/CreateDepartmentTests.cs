using System.Net;
using System.Net.Http.Json;
using DirectoryService.Contracts.DepartmentContracts;
using DirectoryService.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Shared.Result;

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
        var departmentId = await CreateDepartmentViaHttp(locationId: locationId);

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
}