using System.Linq.Expressions;
using CSharpFunctionalExtensions;
using DirectoryService.Domain.Department;
using Shared.Result;

namespace DirectoryService.Application.Abstractions;

public interface IDepartmentRepository
{
    void Add(Department department);
    Task<Result<Department, Error>> GetByAsync(
        Expression<Func<Department, bool>> predicate,
        CancellationToken cancellationToken = default);

    Task<Result<Department, Error>> GetByIdWithLockAsync(
        Guid departmentId,
        CancellationToken cancellationToken = default);

    // блокирует потомков
    Task LockDescendantsAsync(string oldPath, CancellationToken cancellationToken);

    // True, если это дети этого отдела, если это тот и отдел.
    Task<bool> IsDescendantOrSelfAsync(string potentialParentPath, string departmentPath, CancellationToken cancellationToken);

    Task MoveDepartmentAsync(
        string oldPath,
        string newParentPath,
        Guid departmentId,
        Guid? newParentId,
        CancellationToken cancellationToken = default);
    
    Task<bool> AllExistAndActiveAsync(Guid[] ids, CancellationToken cancellationToken = default);
    
    Task<bool> ExistsByIdentifierAsync(string identifier, CancellationToken cancellationToken = default);
    
    Task UpdateLocationsAsync(Department department, CancellationToken cancellationToken = default);
}