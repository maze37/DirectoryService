using System.Net;
using DirectoryService.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace DirectoryService.IntegrationTests.Locations;

public class DeleteLocationsTests : DirectoryBaseTests
{
    public DeleteLocationsTests(DirectoryTestWebFactory factory) : base(factory) { }
    
    [Fact]
    public async Task DeleteLocation_WithLinkedDepartments_SoftDeletes_Returns200()
    {
        // Arrange
        var locationId = await CreateLocationViaHttp();
        await CreateDepartmentViaHttp("test", locationId: locationId);

        // Act
        var response = await Client.DeleteAsync($"/api/locations/{locationId}");

        // Assert — теперь soft delete разрешён даже при наличии связей
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // связь в БД осталась, но локация помечена удалённой
        var location = await ExecuteInDb(async db =>
            await db.Locations
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(l => l.Id == locationId));

        Assert.NotNull(location);
        Assert.True(location.IsDeleted);
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