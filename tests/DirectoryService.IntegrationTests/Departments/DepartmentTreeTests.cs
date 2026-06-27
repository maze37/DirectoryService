using System.Net;
using System.Net.Http.Json;
using DirectoryService.Contracts.DepartmentContracts;
using DirectoryService.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace DirectoryService.IntegrationTests.Departments;

public class DepartmentTreeTests : DirectoryBaseTests
{
    public DepartmentTreeTests(DirectoryTestWebFactory factory) : base(factory) { }

    [Fact]
    public async Task CreateDepartment_WithoutParent_HasDepthZeroAndCorrectPath()
    {
        // Act
        var departmentId = await CreateDepartmentViaHttp(name: "Компания", slug: "kompaniya");

        // Assert DB — проверяем что path = slug, depth = 0
        var department = await ExecuteInDb(async db =>
            await db.Departments.FirstOrDefaultAsync(d => d.Id == departmentId));

        Assert.NotNull(department);
        Assert.Equal("kompaniya", department.Path.Value);
        Assert.Equal(0, department.Depth);
        Assert.Null(department.ParentId);
    }

    [Fact]
    public async Task CreateDepartment_WithParent_HasCorrectPathAndDepth()
    {
        // Arrange — создаём родителя
        var parentId = await CreateDepartmentViaHttp(name: "Компания", slug: "kompaniya");

        // Act — создаём дочерний
        var childId = await CreateDepartmentViaHttp(
            name: "IT-отдел",
            slug: "it", 
            parentId: parentId);

        // Assert DB
        var child = await ExecuteInDb(async db =>
            await db.Departments.FirstOrDefaultAsync(d => d.Id == childId));

        Assert.NotNull(child);
        Assert.Equal("kompaniya.it", child.Path.Value);  // path = parent.slug + "." + slug
        Assert.Equal(1, child.Depth);
        Assert.Equal(parentId, child.ParentId);
    }

    [Fact]
    public async Task CreateDepartment_WithParent_IncrementsParentChildrenCount()
    {
        // Arrange
        var parentId = await CreateDepartmentViaHttp(name: "Компания", slug: "kompaniya");

        // Assert до — у родителя 0 детей
        var parentBefore = await ExecuteInDb(async db =>
            await db.Departments.FirstOrDefaultAsync(d => d.Id == parentId));
        
        Assert.Equal(0, parentBefore!.ChildrenCount);

        // Act — создаём двух детей
        await CreateDepartmentViaHttp(name: "ITTEST", slug: "it", parentId: parentId);
        await CreateDepartmentViaHttp(name: "HRTEST", slug: "hr", parentId: parentId);

        // Assert после — у родителя 2 ребёнка
        var parentAfter = await ExecuteInDb(async db =>
            await db.Departments.FirstOrDefaultAsync(d => d.Id == parentId));
        
        Assert.Equal(2, parentAfter!.ChildrenCount);
    }

    [Fact]
    public async Task CreateDepartment_ThreeLevelsDeep_HasCorrectPathAndDepth()
    {
        // Arrange — строим дерево: Компания - IT - Backend
        var rootId = await CreateDepartmentViaHttp(name: "Компания", slug: "kompaniya");
        var midId  = await CreateDepartmentViaHttp(name: "ITTEST", slug: "it", parentId: rootId);

        // Act
        var leafId = await CreateDepartmentViaHttp(name: "Backend", slug: "backend", parentId: midId);

        // Assert DB
        var leaf = await ExecuteInDb(async db =>
            await db.Departments.FirstOrDefaultAsync(d => d.Id == leafId));

        Assert.NotNull(leaf);
        Assert.Equal("kompaniya.it.backend", leaf.Path.Value);
        Assert.Equal(2, leaf.Depth);
    }

    [Fact]
    public async Task MoveDepartment_ToNewParent_UpdatesPathAndDepth()
    {
        // Arrange — строим: Компания - IT - Backend, и отдельный HR
        var rootId = await CreateDepartmentViaHttp(name: "Компания", slug: "kompaniya");
        var hrId = await CreateDepartmentViaHttp(name: "HRTEST", slug: "hr", parentId: rootId);
        var itId = await CreateDepartmentViaHttp(name: "ITTEST", slug: "it", parentId: rootId);
        var backendId = await CreateDepartmentViaHttp(name: "Backend", slug: "backend", parentId: itId);

        // Act — перемещаем Backend под HR
        var moveRequest = new MoveDepartmentRequest(ParentId: hrId);
        var response = await Client.PutAsJsonAsync(
            $"/api/departments/{backendId}/parent", moveRequest);

        // Assert HTTP
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // Assert DB — путь обновился
        var backend = await ExecuteInDb(async db =>
            await db.Departments.FirstOrDefaultAsync(d => d.Id == backendId));

        Assert.NotNull(backend);
        Assert.Equal("kompaniya.hr.backend", backend.Path.Value);
        Assert.Equal(2, backend.Depth);
        Assert.Equal(hrId, backend.ParentId);
    }

    [Fact]
    public async Task MoveDepartment_ToOwnDescendant_Returns409()
    {
        // Arrange — Компания - IT - Backend
        var rootId = await CreateDepartmentViaHttp(name: "Компания", slug: "kompaniya");
        var itId = await CreateDepartmentViaHttp(name: "ITTEST", slug: "it", parentId: rootId);
        var backendId = await CreateDepartmentViaHttp(name: "Backend", slug: "backend", parentId: itId);

        // Act — пытаемся переместить IT под Backend (его же потомок)
        var moveRequest = new MoveDepartmentRequest(ParentId: backendId);
        var response = await Client.PutAsJsonAsync(
            $"/api/departments/{itId}/parent", moveRequest);

        // Assert — 409 Conflict, зацикливание недопустимо
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task MoveDepartment_ToItself_Returns409()
    {
        // Arrange
        var departmentId = await CreateDepartmentViaHttp(name: "ITTEST", slug: "it");

        // Act — пытаемся переместить отдел под себя
        var moveRequest = new MoveDepartmentRequest(ParentId: departmentId);
        var response = await Client.PutAsJsonAsync(
            $"/api/departments/{departmentId}/parent", moveRequest);

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task MoveDepartment_UpdatesChildrenCountOnBothParents()
    {
        // Arrange
        var rootId = await CreateDepartmentViaHttp(name: "Компания", slug: "kompaniya");
        var itId = await CreateDepartmentViaHttp(name: "ITTEST", slug: "it", parentId: rootId);
        var hrId = await CreateDepartmentViaHttp(name: "HRTEST", slug: "hr", parentId: rootId);
        var backendId = await CreateDepartmentViaHttp(name: "Backend", slug: "backend", parentId: itId);

        // Act — перемещаем Backend из IT в HR
        var moveRequest = new MoveDepartmentRequest(ParentId: hrId);
        await Client.PutAsJsonAsync($"/api/departments/{backendId}/parent", moveRequest);

        // Assert — IT потерял одного ребёнка, HR приобрёл
        var it = await ExecuteInDb(async db =>
            await db.Departments.FirstOrDefaultAsync(d => d.Id == itId));
        var hr = await ExecuteInDb(async db =>
            await db.Departments.FirstOrDefaultAsync(d => d.Id == hrId));

        Assert.Equal(0, it!.ChildrenCount);
        Assert.Equal(1, hr!.ChildrenCount);
    }
}
