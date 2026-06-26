using System.Net;
using System.Net.Http.Json;
using DirectoryService.Contracts.PositionContracts;
using DirectoryService.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Shared.Result;

namespace DirectoryService.IntegrationTests.Position;

public class CreatePositionTests : DirectoryBaseTests
{
    public CreatePositionTests(DirectoryTestWebFactory factory) : base(factory) { }

    [Fact]
    public async Task CreatePosition_WithValidData_ShouldSuccess()
    {
        // Arrange
        var createdDepartmentId = await CreateDepartmentViaHttp("TEST");
        
        // Act
        var position = await CreatePositionViaHttp(departmentId: createdDepartmentId);

        // Assert DB - позиция создалась
        var positionResult = await ExecuteInDb(async db =>
            await db.Positions.FirstOrDefaultAsync(p =>
                p.Id == position));

        Assert.NotNull(positionResult);
        
        // Assert DB - позиция привязана к департаменту
        var departmentPositions = await ExecuteInDb(async db =>
            await db.DepartmentPositions.FirstOrDefaultAsync(dl =>
                dl.DepartmentId == createdDepartmentId &&
                dl.PositionId == position));

        Assert.NotNull(departmentPositions);
    }

    [Fact]
    public async Task CreatePosition_WithEmptyName_ShouldFail()
    {
        // Arrange
        var createdDepartmentId = await CreateDepartmentViaHttp("TEST");

        var request = new CreatePositionRequest(
            Name: "",
            Description: null,
            DepartmentIds: [createdDepartmentId]);

        // Act
        var response = await Client.PostAsJsonAsync("/api/positions/", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreatePosition_WithNonExistingDepartment_ShouldFail()
    {
        // Arrange
        var request = new CreatePositionRequest(
            Name: "testPositionName",
            Description: null,
            DepartmentIds: [Guid.NewGuid()]);

        // Act
        var response = await Client.PostAsJsonAsync("/api/positions/", request);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}