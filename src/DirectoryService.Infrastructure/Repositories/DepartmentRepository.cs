using System.Linq.Expressions;
using System.Net.Http.Headers;
using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Contracts.DepartmentContracts;
using DirectoryService.Domain.Department;
using Microsoft.EntityFrameworkCore;
using Shared.Result;
using Dapper;
using DirectoryService.Domain.Department.ValueObjects;

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

    public async Task<Result<Department, Error>> GetByIdWithLockAsync(
        Guid departmentId, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            var department = await _context.Departments
                    .FromSqlRaw("""
                                SELECT id, parent_id, depth, children_count, is_active, 
                                       created_when, updated_when, name, identifier, path, xmin
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

    public async Task<bool> AllExistAndActiveAsync(Guid[] ids, CancellationToken cancellationToken = default)
    {
        var count = await _context.Departments
            .Where(d => ids.Contains(d.Id) && d.IsActive)
            .CountAsync(cancellationToken);

        return count == ids.Length;
    }

    public async Task<bool> ExistsByIdentifierAsync(string identifier, CancellationToken cancellationToken = default)
    {
        identifier = identifier.Trim();
        
        var identifierValue = Identifier.From(identifier);

        return await _context.Departments
            .AnyAsync(d => d.Identifier == identifierValue, cancellationToken);
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
        string potentialParentPath,
            string departmentPath, 
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
            potentialParentPath,
            departmentPath
        });
        return result;
    }

    public async Task LockDescendantsAsync(string oldPath, CancellationToken cancellationToken = default)
    {
        const string dapperSql = """
                                    SELECT 1 FROM departments
                                    WHERE path <@ @oldPath::ltree
                                    FOR UPDATE
                                 """;

        var dbConn = _context.Database.GetDbConnection();
        await dbConn.ExecuteAsync(new CommandDefinition(
            dapperSql, 
            new { oldPath }, 
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
}