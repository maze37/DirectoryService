using System.Linq.Expressions;
using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Domain.Department;
using Microsoft.EntityFrameworkCore;
using Shared.Result;
using Dapper;
using DirectoryService.Domain.Department.ValueObjects;
using DirectoryService.Domain.DepartmentPositions;
using Path = DirectoryService.Domain.Department.ValueObjects.Path;

namespace DirectoryService.Infrastructure.Repositories;

public class DepartmentRepository : IDepartmentRepository
{
    private readonly AppDbContext _context;

    public DepartmentRepository(AppDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public void Add(Department department)
    {
        _context.Departments.Add(department);
    }

    public async Task<Result<Department, Error>> GetByAsync(
        Expression<Func<Department, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var department = await _context.Departments
                .FirstOrDefaultAsync(predicate, cancellationToken);

            if (department is null)
                return Errors.General.NotFound(name: "department");

            return department;
        }
        catch (Exception)
        {
            return Error.Failure("department.get.failed", "Не удалось получить подразделение");
        }
    }

    public async Task<Result<Department, Error>> GetByIdWithLock(
        Guid departmentId, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            var department = await _context.Departments
                    .FromSqlRaw("""
                                SELECT id, parent_id, depth, children_count, is_active, 
                                       created_when, updated_when, name, slug, path, xmin
                                FROM departments 
                                WHERE id = {0} 
                                FOR UPDATE
                                """, departmentId)
                    .FirstOrDefaultAsync(cancellationToken);

            if (department is null)
                return Errors.General.NotFound(name: "department");

            return department;
        }
        catch (Exception ex)
        {
            return Error.Failure("department.get.failed", ex.Message);
        }
    }

    public async Task DeleteWithDescendants(Path path, CancellationToken cancellationToken)
    {
        const string dapperSql = """
                                    DELETE FROM departments
                                    WHERE path <@ @path::ltree
                                 """;

        var dbConn = _context.Database.GetDbConnection();
        await dbConn.ExecuteAsync(new CommandDefinition(
            dapperSql, 
            new { path = path.Value }, 
            cancellationToken: cancellationToken));
    }

    public async Task<bool> AllExistAndActiveAsync(Guid[] ids, CancellationToken cancellationToken = default)
    {
        var count = await _context.Departments
            .Where(d => ids.Contains(d.Id) && d.IsActive)
            .CountAsync(cancellationToken);

        return count == ids.Length;
    }

    public async Task<bool> ExistsBySlugWithLockAsync(string slug, CancellationToken cancellationToken)
    {
        const string sql = "SELECT 1 FROM departments WHERE slug = @slug FOR UPDATE";
    
        var dbConn = _context.Database.GetDbConnection();
        // Здесь важно: если _context в транзакции, то dbConn тоже в ней
        var result = await dbConn.ExecuteScalarAsync<int?>(
            new CommandDefinition(sql, new { slug }, cancellationToken: cancellationToken));
    
        return result.HasValue;
    }

    public async Task UpdateLocationsAsync(
        Department department, 
        CancellationToken cancellationToken = default)
    {
        var existing = await _context.DepartmentLocations
            .Where(dl => dl.DepartmentId == department.Id)
            .ToListAsync(cancellationToken);

        _context.DepartmentLocations.RemoveRange(existing);
        
        await _context.DepartmentLocations.AddRangeAsync(department.Locations, cancellationToken);
    }

    /// <summary>
    /// Вернет false, если не потомок и не сам department.
    /// </summary>
    public async Task<bool> IsDescendantOrSelfAsync(
        Path potentialParentPath,
        Path departmentPath, 
        CancellationToken cancellationToken = default)
    {
        const string dapperSql = """
                                    SELECT EXISTS (
                                        SELECT 1 
                                        WHERE @potentialParentPath::ltree <@ @departmentPath::ltree
                                    )
                                 """;
        
        var dbConn = _context.Database.GetDbConnection();
        // для одного значения (bool) в даппере можно юзать ExecuteScalarAsync
        var result = await dbConn.ExecuteScalarAsync<bool>(dapperSql, new
        {
            potentialParentPath = potentialParentPath.Value,
            departmentPath = departmentPath.Value
        });
        return result;
    }

    public async Task LockDescendantsAsync(Path oldPath, CancellationToken cancellationToken = default)
    {
        const string dapperSql = """
                                    SELECT 1 FROM departments
                                    WHERE path <@ @oldPath::ltree
                                    FOR UPDATE
                                 """;

        var dbConn = _context.Database.GetDbConnection();
        await dbConn.ExecuteAsync(new CommandDefinition(
            dapperSql, 
            new { oldPath = oldPath.Value }, 
            cancellationToken: cancellationToken));
    }

    public async Task MoveDepartmentAsync(
        string oldPath, 
        string newParentPath, 
        Guid departmentId, 
        Guid? newParentId,
        CancellationToken cancellationToken = default)
    {
        const string dapperSql = """
                                 UPDATE departments
                                 SET
                                     path = @newParentPath::ltree || subpath(path, nlevel(@oldPath::ltree) - 1),
                                     depth = nlevel(@newParentPath::ltree || subpath(path, nlevel(@oldPath::ltree) - 1)) - 1,
                                     parent_id = CASE
                                        WHEN id = @departmentId THEN @newParentId
                                        ELSE parent_id
                                        END
                                 WHERE path <@ @oldPath::ltree
                                 """;

        var dbConn = _context.Database.GetDbConnection();
        await dbConn.ExecuteAsync(new CommandDefinition(
            dapperSql,
            new { newParentPath, oldPath, departmentId, newParentId },
            cancellationToken: cancellationToken));
    }

    public async Task<bool> IsPositionLinkedAsync(
        Guid departmentId,
        Guid positionId,
        CancellationToken cancellationToken = default)
    {
        var isLinked = await _context.DepartmentPositions
            .AnyAsync(x => x.DepartmentId == departmentId && x.PositionId == positionId);

        return isLinked;
    }

    public async Task<Result<DepartmentPosition, Error>> GetPositionLinkAsync(
        Guid departmentId,
        Guid positionId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var departmentPosition = await _context.DepartmentPositions
                .FirstOrDefaultAsync(x => x.DepartmentId == departmentId && x.PositionId == positionId, cancellationToken);
            
            if (departmentPosition is null)
                return Errors.General.NotFound(name: "department.position");

            return departmentPosition;
        }
        catch (Exception ex)
        {
            return Error.Failure("department.position.get.failed", ex.Message);
        }
    }
    

    public void AddPositionLink(DepartmentPosition link)
    {
        _context.DepartmentPositions.Add(link);
    }

    public void RemovePositionLink(DepartmentPosition link)
    {
        _context.DepartmentPositions.Remove(link);
    }
}