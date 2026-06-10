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
    /// Проверяет существует ли активная должность с указанным названием.
    /// </summary>
    Task<bool> ExistsActiveWithNameAsync(string name, CancellationToken cancellationToken = default);
}