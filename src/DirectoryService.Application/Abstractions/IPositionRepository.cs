using CSharpFunctionalExtensions;
using DirectoryService.Domain.Position;

namespace DirectoryService.Application.Abstractions;

/// <summary>
/// Репозиторий для работы с должностями.
/// </summary>
public interface IPositionRepository
{
    /// <summary>
    /// Добавляет должность в ChangeTracker.
    /// </summary>
    void Add(Position position);

    /// <summary>
    /// Помечает должность в ChangeTracker удаленным.
    /// </summary>
    void Remove(Position position);

    /// <summary>
    /// Проверяет, есть ли у отделов такая должность.
    /// </summary>
    Task<bool> HasDepartmentLinksAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>
    /// Проверяет существует ли активная должность с указанным названием.
    /// </summary>
    Task<bool> ExistsActiveWithNameAsync(string name, CancellationToken cancellationToken = default);

    /// <summary>
    /// Возвращает Position с блокировкой.
    /// </summary>
    Task<Result<Position, Error>> GetByIdWithLock(Guid positionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Добавить обновленную сущность Position в ChangeTracker.
    /// </summary>
    /// <param name="position"></param>
    void Update(Position position);
    
    Task<int> DeleteSoftDeletedBatchAsync(
        DateTime olderThanUtc,
        int batchSize,
        CancellationToken cancellationToken);

    Task<Result<Position, Error>> GetDeletedByIdWithLock(
        Guid positionId,
        CancellationToken cancellationToken = default);
}