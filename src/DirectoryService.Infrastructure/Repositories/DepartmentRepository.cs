using System.Linq.Expressions;
using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Domain.Department;
using Microsoft.EntityFrameworkCore;
using Shared.Result;

namespace DirectoryService.Infrastructure.Repositories;

public class DepartmentRepository : IDepartmentRepository
{
    private readonly AppDbContext _context;

    public DepartmentRepository(AppDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task AddAsync(Department department, CancellationToken cancellationToken = default)
    {
        await _context.Departments.AddAsync(department, cancellationToken);
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

        return await _context.Departments
            .AnyAsync(d => d.Identifier.Value == identifier, cancellationToken);
    }
}
