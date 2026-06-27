using System.Net;
using System.Net.Http.Json;
using DirectoryService.Contracts.Constants;
using DirectoryService.Contracts.DepartmentContracts;
using DirectoryService.IntegrationTests.Infrastructure;
using Shared.Result;

namespace DirectoryService.IntegrationTests.Departments;

public class GetDepartmentsTests : DirectoryBaseTests
{
    public GetDepartmentsTests(DirectoryTestWebFactory factory) : base(factory) { }

    [Fact]
    public async Task GetDepartments_WithoutParams_ReturnsFirstPageWithDefaults()
    {
        // Arrange — создаём 3 отдела
        await CreateDepartmentViaHttp(slug: "alpha");
        await CreateDepartmentViaHttp(slug: "beta");
        await CreateDepartmentViaHttp(slug: "gamma");

        // Act
        var response = await Client.GetAsync("/api/departments?Pagination.Page=1&Pagination.PageSize=20");
        var envelope = await response.Content.ReadFromJsonAsync<Envelope<PagedResult<DepartmentListItemDto>>>();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(envelope?.Result);
        Assert.Equal(3, envelope.Result.TotalCount);
        Assert.Equal(3, envelope.Result.Items.Count);
        Assert.Equal(1, envelope.Result.Pagination.Page);
        Assert.Equal(20, envelope.Result.Pagination.PageSize);
    }

    [Fact]
    public async Task GetDepartments_WithSearch_ReturnsOnlyMatchingItems()
    {
        // Arrange
        await CreateDepartmentViaHttp(name: "IT-department", slug: "itdepartment");
        await CreateDepartmentViaHttp(name: "HR-department", slug: "hrdepartment");
        await CreateDepartmentViaHttp(name: "FinanceOtdel", slug: "finance");

        // Act — ищем по подстроке "depart"
        var response = await Client.GetAsync(
            "/api/departments?Pagination.Page=1&Pagination.PageSize=20&search=depart");
        var envelope = await response.Content.ReadFromJsonAsync<Envelope<PagedResult<DepartmentListItemDto>>>();

        // Assert — должны найти только IT и HR
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(envelope?.Result);
        Assert.Equal(2, envelope.Result.TotalCount);
        Assert.All(envelope.Result.Items, item =>
            Assert.Contains("depart", item.Name, StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task GetDepartments_SearchIsCaseInsensitive()
    {
        // Arrange
        await CreateDepartmentViaHttp(slug: "itdepartment");

        // Act — ищем строчными буквами
        var response = await Client.GetAsync(
            "/api/departments?Pagination.Page=1&Pagination.PageSize=20&search=depart");
        var envelope = await response.Content.ReadFromJsonAsync<Envelope<PagedResult<DepartmentListItemDto>>>();

        // Assert — должен найти, несмотря на разный регистр
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(1, envelope!.Result!.TotalCount);
    }

    [Fact]
    public async Task GetDepartments_WithSortByNameDesc_ReturnsSortedItems()
    {
        // Arrange
        await CreateDepartmentViaHttp(name: "Alpha", slug: "alpha");
        await CreateDepartmentViaHttp(name: "Gamma", slug: "gamma");
        await CreateDepartmentViaHttp(name: "Beta", slug: "beta");

        // Act
        var response = await Client.GetAsync("/api/departments?Pagination.Page=1&Pagination.PageSize=20&sortBy=name&sortDir=desc");
        var envelope = await response.Content.ReadFromJsonAsync<Envelope<PagedResult<DepartmentListItemDto>>>();

        // Assert — должны быть в порядке Г - Б - А
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var items = envelope!.Result!.Items;
        Assert.Equal("Gamma", items[0].Name);
        Assert.Equal("Beta",  items[1].Name);
        Assert.Equal("Alpha", items[2].Name);
    }

    [Fact]
    public async Task GetDepartments_WithInvalidSortBy_Returns400()
    {
        // Act
        var response = await Client.GetAsync("/api/departments?Pagination.Page=1&Pagination.PageSize=20&sortBy=password");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetDepartments_WithInvalidSortDir_Returns400()
    {
        // Act
        var response = await Client.GetAsync("/api/departments?Pagination.Page=1&Pagination.PageSize=20&sortBy=name&sortDir=random");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetDepartments_PageSizeExceeds100_Returns400()
    {
        // Act
        var response = await Client.GetAsync("/api/departments?Pagination.Page=1&Pagination.PageSize=101");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetDepartments_PageLessThan1_Returns400()
    {
        // Act
        var response = await Client.GetAsync("/api/departments?Pagination.Page=0");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetDepartments_EmptyResult_Returns200WithEmptyList()
    {
        // Arrange — ничего не создаём, база пустая после Respawn

        // Act
        var response = await Client.GetAsync(
            "/api/departments?Pagination.Page=1&Pagination.PageSize=20&search=nonexistent");
        var envelope = await response.Content.ReadFromJsonAsync<Envelope<PagedResult<DepartmentListItemDto>>>();

        // Assert — 200 с пустым списком, не 404
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(envelope?.Result);
        Assert.Equal(0, envelope.Result.TotalCount);
        Assert.Empty(envelope.Result.Items);
    }
}