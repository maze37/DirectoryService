using DirectoryService.Application.Abstractions;
using DirectoryService.Domain.Department;
using Microsoft.EntityFrameworkCore;

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

    public async Task<Department?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Departments
            .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
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
        return await _context.Departments
            .AnyAsync(d => d.Identifier.Value == identifier, cancellationToken);
    }
}
