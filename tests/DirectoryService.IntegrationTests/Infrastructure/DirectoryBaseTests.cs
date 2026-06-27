using System.Net.Http.Json;
using DirectoryService.Contracts.DepartmentContracts;
using DirectoryService.Contracts.LocationContracts;
using DirectoryService.Contracts.PositionContracts;
using DirectoryService.Infrastructure;
using Microsoft.AspNetCore.Mvc.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Shared.Result;

namespace DirectoryService.IntegrationTests.Infrastructure;

public class DirectoryBaseTests : IClassFixture<DirectoryTestWebFactory>, IAsyncLifetime
{
    protected IServiceProvider Services { get; set; }
    private readonly Func<Task> _resetDatabase;
    protected HttpClient Client { get; }

    protected DirectoryBaseTests(DirectoryTestWebFactory factory)
    {
        Services = factory.Services;
        Client = factory.CreateClient();
        _resetDatabase = factory.ResetDatabaseAsync;
    }
    
    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        await _resetDatabase();
    }

    protected async Task<T> ExecuteInDb<T>(Func<AppDbContext, Task<T>> action)
    {
        await using var scope = Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        return await action(dbContext);
    }

    protected async Task ExecuteInDb(Func<AppDbContext, Task> action)
    {
        await using var scope = Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await action(dbContext);
    }
    
    protected async Task<Guid> CreateLocationViaHttp(
        string name = "Test",
        AddressDto address = null!,
        string timeZone = "Europe/Moscow")
    {
        var request = new CreateLocationRequest(name, address ?? DefaultAddress, timeZone);

        var response = await Client.PostAsJsonAsync("/api/locations", request);
        response.EnsureSuccessStatusCode();

        var envelope = await response.Content.ReadFromJsonAsync<Envelope<CreateLocationResponse>>();
        return envelope!.Result!.Id;
    }
    
    protected static AddressDto DefaultAddress => new()
    {
        Country = "Test",
        City = "Test",
        Street = "Test",
        Building = "1"
    };

    protected async Task<Guid> CreateDepartmentViaHttp(
        string slug,
        string name = "Test Department",
        Guid? parentId = null,
        Guid? locationId = null)
    {
        var locId = locationId ?? await CreateLocationViaHttp("Нукус");

        var request = new CreateDepartmentRequest(
            Name: name,
            Slug: slug,
            ParentId: parentId,
            LocationIds: [locId]);

        // var response = await Client.PostAsJsonAsync("/api/departments", request);
        // response.EnsureSuccessStatusCode();
        
        var response = await Client.PostAsJsonAsync("/api/departments", request);

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            throw new Exception(body);
        }

        var envelope = await response.Content.ReadFromJsonAsync<Envelope<CreateDepartmentResponse>>();
        return envelope!.Result!.Id;
    }

    protected async Task<Guid> CreatePositionViaHttp(
        string name = "Test",
        string? description = "",
        Guid? departmentId = null)
    {
        var depId = departmentId ?? await CreateDepartmentViaHttp("test");

        var request = new CreatePositionRequest(
            Name: name,
            Description: description,
            DepartmentIds: [depId]);
        
        var response = await Client.PostAsJsonAsync("/api/positions/", request);
        response.EnsureSuccessStatusCode();
        
        var envelope = await response.Content.ReadFromJsonAsync<Envelope<CreatePositionResponse>>();
        return envelope!.Result!.Id;
    }
}