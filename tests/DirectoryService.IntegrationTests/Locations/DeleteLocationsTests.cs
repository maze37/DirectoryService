using System.Net;
using DirectoryService.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace DirectoryService.IntegrationTests.Locations;

public class DeleteLocationsTests : DirectoryBaseTests
{
    public DeleteLocationsTests(DirectoryTestWebFactory factory) : base(factory) { }
    
    [Fact]
    public async Task DeleteLocation_WithLinkedDepartments_Returns409()
    {
        var locationId = await CreateLocationViaHttp("Офис");
        await CreateDepartmentViaHttp(slug: "latintest", locationId: locationId);

        var response = await Client.DeleteAsync($"/api/locations/{locationId}");

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task DeleteLocation_WithoutLinks_Succeeds()
    {
        var locationId = await CreateLocationViaHttp("Пустой офис");

        var response = await Client.DeleteAsync($"/api/locations/{locationId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var location = await ExecuteInDb(async db =>
            await db.Locations.FirstOrDefaultAsync(l => l.Id == locationId));
        Assert.Null(location);
    }
}