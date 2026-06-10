using System.Linq.Expressions;
using CSharpFunctionalExtensions;
using DirectoryService.Domain.Department;
using Path = DirectoryService.Domain.Department.ValueObjects.Path;

namespace DirectoryService.Application.Abstractions;

/// <summary>
/// Репозиторий для работы с подразделениями.
/// </summary>
public interface IDepartmentRepository
{
    /// <summary>
    /// Добавляет подразделение в ChangeTracker.
    /// </summary>
    void Add(Department department);

    /// <summary>
    /// Возвращает подразделение по предикату.
    /// </summary>
    Task<Result<Department, Error>> GetByAsync(
        Expression<Func<Department, bool>> predicate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Возвращает подразделение по ID с пессимистичной блокировкой (FOR UPDATE).
    /// </summary>
    Task<Result<Department, Error>> GetByIdWithLockAsync(
        Guid departmentId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Блокирует подразделение и всех его потомков (FOR UPDATE) по пути.
    /// </summary>
    Task LockDescendantsAsync(Path oldPath, CancellationToken cancellationToken);

    /// <summary>
    /// Возвращает true если potentialParentPath является потомком departmentPath или совпадает с ним.
    /// Используется для проверки зацикливания при переносе подразделения.
    /// </summary>
    Task<bool> IsDescendantOrSelfAsync(
        Path potentialParentPath, 
        Path departmentPath, 
        CancellationToken cancellationToken);

    /// <summary>
    /// Переносит подразделение и всех его потомков на новый путь через массовый SQL UPDATE.
    /// </summary>
    Task MoveDepartmentAsync(
        string oldPath,
        string newParentPath,
        Guid departmentId,
        Guid? newParentId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Проверяет что все подразделения с указанными ID существуют и активны.
    /// </summary>
    Task<bool> AllExistAndActiveAsync(Guid[] ids, CancellationToken cancellationToken = default);

    /// <summary>
    /// Проверяет существует ли подразделение с указанным идентификатором.
    /// </summary>
    Task<bool> ExistsByIdentifierAsync(string identifier, CancellationToken cancellationToken = default);

    /// <summary>
    /// Обновляет список локаций подразделения в ChangeTracker.
    /// </summary>
    Task UpdateLocationsAsync(Department department, CancellationToken cancellationToken = default);
}