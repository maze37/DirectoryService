using System.Net;
using DirectoryService.Contracts.DepartmentContracts;
using DirectoryService.IntegrationTests.Infrastructure;

namespace DirectoryService.IntegrationTests.Departments;

public class GetDepartmentTreeTests : DirectoryBaseTests
{
    public GetDepartmentTreeTests(DirectoryTestWebFactory factory) : base(factory) { }
    
    [Fact]
    public async Task GetTree_EmptyDatabase_ReturnsEmptyList()
    {
        // Arrange

        // Act
        var response = await Client.GetAsync("/api/departments/tree");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
    
    [Fact]
    public async Task GetChildren_NodeWithNoChildren_ReturnsEmptyList()
    {
        // Arrange
        var rootId = await CreateDepartmentViaHttp("test");

        // Act
        var response = await Client.GetAsync($"api/departments/{rootId}/children");
        
        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
    
    [Fact]
    public async Task GetChildren_NonExistentNode_Returns404()
    {
        // Arrange
        var randomGuid = Guid.NewGuid();
        
        // Act
        var response = await Client.GetAsync($"/api/departments/{randomGuid}/children");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
    
    [Fact]
    public async Task GetAncestors_RootNode_ReturnsEmptyList()
    {
        // Arrange
        var departmentId = await CreateDepartmentViaHttp("TEST");
    
        // Act
        var response = await Client.GetAsync($"/api/departments/{departmentId}/ancestors");
        
        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
    
    [Fact]
    public async Task Search_NoMatches_ReturnsEmptyList()
    {
        // Arrange
        await CreateDepartmentViaHttp("TEST");
        
        // Act
        var response = await Client.GetAsync("/api/departments/tree/search?q=xyz");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
    
    [Fact]
    public async Task Search_QueryTooShort_Returns400()
    {
        // Act
        var response = await Client.GetAsync("/api/departments/tree/search?q=a");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}