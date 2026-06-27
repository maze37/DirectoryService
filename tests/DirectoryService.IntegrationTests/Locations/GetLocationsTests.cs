using System.Net;
using System.Net.Http.Json;
using DirectoryService.Contracts.Constants;
using DirectoryService.Contracts.LocationContracts;
using DirectoryService.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Shared.Result;

namespace DirectoryService.IntegrationTests.Locations;

public class GetLocationsTests : DirectoryBaseTests
{
    public GetLocationsTests(DirectoryTestWebFactory factory) : base(factory) { }

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
    
    [Fact]
    public async Task GetLocations_WithoutParams_ReturnsAllLocations()
    {
        // Arrange
        await CreateLocationViaHttp("Москва");
        await CreateLocationViaHttp("Питер");

        // Act
        var response = await Client.GetAsync("/api/locations?Pagination.Page=1&Pagination.PageSize=20");
        var envelope = await response.Content.ReadFromJsonAsync<Envelope<PagedResult<LocationListItemDto>>>();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(envelope?.Result);
        Assert.Equal(2, envelope.Result.TotalCount);
        Assert.Equal(2, envelope.Result.Items.Count);
    }

    [Fact]
    public async Task GetLocations_DepartmentCountIsCorrect()
    {
        // Arrange — создаём локацию и два отдела, привязанных к ней
        var locationId = await CreateLocationViaHttp("Москва");
        await CreateDepartmentViaHttp(slug: "testq", locationId: locationId);
        await CreateDepartmentViaHttp(slug: "testw", locationId: locationId);

        // Act
        var response = await Client.GetAsync("/api/locations?Pagination.Page=1&Pagination.PageSize=20");
        var envelope = await response.Content.ReadFromJsonAsync<Envelope<PagedResult<LocationListItemDto>>>();

        // Assert — у локации должно быть 2 отдела
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var location = envelope!.Result!.Items.Single(l => l.Name == "Москва");
        Assert.Equal(2, location.DepartmentCount);
    }

    [Fact]
    public async Task GetLocations_WithMinDepartmentCount_FiltersCorrectly()
    {
        // Arrange
        var busyLocation = await CreateLocationViaHttp("Загруженный офис");
        var emptyLocation = await CreateLocationViaHttp("Пустой офис");

        // К первой локации привязываем 3 отдела, ко второй — ни одного
        await CreateDepartmentViaHttp(slug: "testq", locationId: busyLocation);
        await CreateDepartmentViaHttp(slug: "testw", locationId: busyLocation);
        await CreateDepartmentViaHttp(slug: "teste", locationId: busyLocation);

        // Act — запрашиваем только локации с минимум 2 отделами
        var response = await Client.GetAsync(
            "/api/locations?MinDepartmentCount=2&Pagination.Page=1&Pagination.PageSize=20");
        var envelope = await response.Content.ReadFromJsonAsync<Envelope<PagedResult<LocationListItemDto>>>();

        // Assert — только загруженный офис прошёл фильтр
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(1, envelope!.Result!.TotalCount);
        Assert.Equal("Загруженный офис", envelope.Result.Items.Single().Name);
    }

    [Fact]
    public async Task GetLocations_WithNegativeMinDepartmentCount_Returns400()
    {
        // Act
        var response = await Client.GetAsync(
            "/api/locations?MinDepartmentCount=-1&Pagination.Page=1&Pagination.PageSize=20");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetLocations_AddressIsReturnedCorrectly()
    {
        // Arrange — создаём локацию с конкретным адресом
        var address = new AddressDto
        {
            Country = "Россия",
            City = "Москва",
            Street = "Тверская",
            Building = "1",
            Office = "101",
            PostalCode = "125009"
        };
        await CreateLocationViaHttp("Главный офис", address);

        // Act
        var response = await Client.GetAsync("/api/locations?Pagination.Page=1&Pagination.PageSize=20");
        var envelope = await response.Content.ReadFromJsonAsync<Envelope<PagedResult<LocationListItemDto>>>();

        // Assert — адрес должен замапиться корректно
        var location = envelope!.Result!.Items.Single();
        Assert.Equal("Россия", location.Address.Country);
        Assert.Equal("Москва", location.Address.City);
        Assert.Equal("Тверская", location.Address.Street);
        Assert.Equal("1", location.Address.Building);
        Assert.Equal("101", location.Address.Office);
        Assert.Equal("125009", location.Address.PostalCode);
    }

    [Fact]
    public async Task GetLocations_WithSortByDepartmentCountDesc_ReturnsSortedCorrectly()
    {
        // Arrange
        var bigOffice   = await CreateLocationViaHttp("Большой офис");
        var smallOffice = await CreateLocationViaHttp("Маленький офис");

        await CreateDepartmentViaHttp(slug: "testq", locationId: bigOffice);
        await CreateDepartmentViaHttp(slug: "testw", locationId: bigOffice);
        await CreateDepartmentViaHttp(slug: "teste", locationId: bigOffice);
        await CreateDepartmentViaHttp(slug: "testr", locationId: smallOffice);

        // Act
        var response = await Client.GetAsync(
            "/api/locations?sortBy=departmentCount&sortDir=desc&Pagination.Page=1&Pagination.PageSize=20");
        var envelope = await response.Content.ReadFromJsonAsync<Envelope<PagedResult<LocationListItemDto>>>();

        // Assert — первый должен быть большой офис (3 отдела)
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var items = envelope!.Result!.Items;
        Assert.Equal("Большой офис", items[0].Name);
        Assert.Equal(3, items[0].DepartmentCount);
        Assert.Equal("Маленький офис", items[1].Name);
        Assert.Equal(1, items[1].DepartmentCount);
    }

    [Fact]
    public async Task GetLocations_TotalCountReflectsFilter_NotPageSize()
    {
        // Arrange — создаём 25 локаций
        for (var i = 1; i <= 25; i++)
            await CreateLocationViaHttp($"Офис {i:D2}");

        // Act — запрашиваем первую страницу из 10
        var response = await Client.GetAsync(
            "/api/locations?Pagination.Page=1&Pagination.PageSize=10");
        var envelope = await response.Content.ReadFromJsonAsync<Envelope<PagedResult<LocationListItemDto>>>();

        // Assert — Items 10 (размер страницы), но TotalCount 25 (все под фильтром)
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(25, envelope!.Result!.TotalCount); // totalCount = все, не страница
        Assert.Equal(10, envelope.Result.Items.Count);  // Items = только текущая страница
    }
}
