namespace DirectoryService.Application.Abstractions;

/// <summary>
/// UnitOfWork Паттерн
/// </summary>
public interface IUnitOfWork
{
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}