using DirectoryService.Domain.Department;

namespace DirectoryService.Application.Abstractions;

public interface IDepartmentRepository
{
    Task AddAsync(Department department, CancellationToken cancellationToken = default);
    Task<Department?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> AllExistAndActiveAsync(Guid[] ids, CancellationToken cancellationToken = default);
    Task<bool> ExistsByIdentifierAsync(string identifier, CancellationToken cancellationToken = default);
}
