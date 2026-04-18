using DirectoryService.Domain.Department;

namespace DirectoryService.Application.Abstractions;

public interface IDepartmentRepository
{
    Task AddAsync(Department department, CancellationToken cancellationToken = default);
    Task<Department?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}