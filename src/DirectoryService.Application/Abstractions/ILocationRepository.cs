using DirectoryService.Domain.Location;

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
}