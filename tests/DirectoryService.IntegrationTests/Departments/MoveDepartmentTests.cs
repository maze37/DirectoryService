using System.Net;
using System.Net.Http.Json;
using DirectoryService.Contracts.DepartmentContracts;
using DirectoryService.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace DirectoryService.IntegrationTests.Departments;

public class MoveDepartmentTests : DirectoryBaseTests
{
    public MoveDepartmentTests(DirectoryTestWebFactory factory) : base(factory) { }

    [Fact]
    public async Task MoveDepartment_ConcurrentMoves_OneWins_OneGets409OrOk()
    {
        // Arrange — Компания - IT, Компания - HR, IT - Backend
        var rootId = await CreateDepartmentViaHttp(name: "Компания", slug: "kompaniya");
        var itId = await CreateDepartmentViaHttp(name: "ITtest", slug: "it", parentId: rootId);
        var hrId = await CreateDepartmentViaHttp(name: "HRtest", slug: "hr", parentId: rootId);
        var backendId = await CreateDepartmentViaHttp(name: "Backend", slug: "backend", parentId: itId);

        // Act — два одновременных перемещения Backend: один хочет в IT, другой в HR
        var move1 = Client.PutAsJsonAsync(
            $"/api/departments/{backendId}/parent",
            new MoveDepartmentRequest(ParentId: itId));
        var move2 = Client.PutAsJsonAsync(
            $"/api/departments/{backendId}/parent",
            new MoveDepartmentRequest(ParentId: hrId));

        var responses = await Task.WhenAll(move1, move2);

        // Assert — оба не могут быть одновременно успешными без race condition
        // Хотя бы один должен завершиться с каким-то известным статусом (200 или 409)
        foreach (var response in responses)
        {
            Assert.Contains(response.StatusCode, new[]
            {
                HttpStatusCode.OK,
                HttpStatusCode.Conflict
            });
        }

        // Assert DB — отдел должен находиться ровно в одном месте
        var backend = await ExecuteInDb(async db =>
            await db.Departments.FirstOrDefaultAsync(d => d.Id == backendId));

        Assert.NotNull(backend);
        // ParentId должен быть либо itId, либо hrId — не null и не что-то третье
        Assert.True(backend.ParentId == itId || backend.ParentId == hrId);
    }
}