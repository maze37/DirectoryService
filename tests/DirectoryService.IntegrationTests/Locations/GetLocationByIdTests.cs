using System.Net;
using System.Net.Http.Json;
using DirectoryService.Contracts.LocationContracts;
using DirectoryService.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Shared.Result;

namespace DirectoryService.IntegrationTests.Locations;

public class GetLocationByIdTests : DirectoryBaseTests
{
    public GetLocationByIdTests(DirectoryTestWebFactory factory) : base(factory) { }

    [Fact]
    public async Task GetLocationById_WithExistingId_Returns200AndCorrectData()
    {
        // Arrange
        var correctId = await CreateLocationViaHttp(name: "Нукус");

        // Act — GET запрос, id в URL
        var response = await Client.GetAsync($"/api/locations/{correctId}");
        var envelope = await response.Content.ReadFromJsonAsync<Envelope<GetLocationDto>>();

        // Assert HTTP
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(envelope);
        Assert.Equal(correctId, envelope.Result!.Id);

        // Assert DB
        var location = await ExecuteInDb(async db =>
            await db.Locations.FirstOrDefaultAsync(l => l.Id == envelope.Result.Id));

        Assert.NotNull(location);
    }

    [Fact]
    public async Task GetLocationById_WithWrongId_Returns404()
    {
        // Arrange
        var wrongId = Guid.NewGuid();

        // Act
        var response = await Client.GetAsync($"/api/locations/{wrongId}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}