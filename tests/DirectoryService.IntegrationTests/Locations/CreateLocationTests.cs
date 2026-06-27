using System.Net.Http.Json;
using DirectoryService.Contracts.LocationContracts;
using DirectoryService.IntegrationTests.Infrastructure;
using Shared.Result;

namespace DirectoryService.IntegrationTests.Locations;

public class CreateLocationTests : DirectoryBaseTests
{
    public CreateLocationTests(DirectoryTestWebFactory factory) : base(factory) { }

    [Fact]
    public async Task CreateLocation_ShouldSuccess()
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
}
