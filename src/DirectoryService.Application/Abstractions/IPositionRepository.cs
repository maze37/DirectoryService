using DirectoryService.Domain.Position;

namespace DirectoryService.Application.Abstractions;

public interface IPositionRepository
{
    Task AddAsync(Position position, CancellationToken cancellationToken = default);
    Task<bool> ExistsActiveWithNameAsync(string name, CancellationToken cancellationToken = default);
}