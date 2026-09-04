using CSharpFunctionalExtensions;
using DirectoryService.Domain.Location;
using Shared.Result;

namespace DirectoryService.Application.Abstractions;

/// <summary>
/// Репозиторий для работы с локациями.
/// </summary>
public interface ILocationRepository
{
    /// <summary>
    /// Добавляет локацию в ChangeTracker.
    /// </summary>
    void Add(Location location);

    /// <summary>
    /// Проверяет что все локации с указанными ID существуют и активны.
    /// </summary>
    Task<bool> AllExistAsync(IReadOnlyList<Guid> locationIds, CancellationToken cancellationToken = default);

    /// <summary>
    /// Вернет Location с пес. блокировкой.
    /// </summary>
    Task<Result<Location, Error>> GetByIdWithLock(Guid id, CancellationToken cancellationToken);

    /// <summary>
    /// Проверяет, есть ли привязки к отделам у локаици.
    /// </summary>
    Task<bool> HasDepartmentLinksAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>
    /// Помечает локацию удаленной в ChangeTracker.
    /// </summary>
    void Remove(Location location);
    
    Task<int> DeleteSoftDeletedBatchAsync(
        DateTimeOffset olderThanUtc,
        int batchSize,
        CancellationToken cancellationToken);

    Task<Result<Location, Error>> GetDeletedByIdWithLock(
        Guid locationsId,
        CancellationToken cancellationToken = default);
}