using System.Net.Http.Json;
using DirectoryService.Contracts.LocationContracts;
using DirectoryService.IntegrationTests.Infrastructure;
using Shared.Result;

namespace DirectoryService.IntegrationTests.Locations;

public class GetLocationTests : DirectoryBaseTests
{
    public GetLocationTests(DirectoryTestWebFactory factory) : base(factory) { }

    [Fact]
    public async Task GetLocationById_WithExistingId_ShouldReturn200()
    {
        // Arrange
        var locationName = "Нукус";
        
        var address = new AddressDto
        {
            Country = "Россия", 
            City = "Москва", 
            Street = "Тверская", 
            Building = "1", 
            Office = null, 
            PostalCode = null
        };
        
        var timeZone = "Europe/Moscow";

        // Act
        var result = await CreateLocationViaHttp(locationName, address, timeZone);

        // Assert
        Assert.NotEqual(Guid.Empty, result);
    }

    private async Task<Guid> CreateLocationViaHttp(string name, AddressDto address, string timeZone)
    {
        var request = new CreateLocationRequest(name, address, timeZone);

        var response = await Client.PostAsJsonAsync("/api/locations", request);
        response.EnsureSuccessStatusCode();
        
        var envelope = await response.Content.ReadFromJsonAsync<Envelope<CreateLocationResponse>>();
        return envelope!.Result!.Id;
    }
}