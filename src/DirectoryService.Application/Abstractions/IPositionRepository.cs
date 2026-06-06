using DirectoryService.Domain.Position;

namespace DirectoryService.Application.Abstractions;

public interface IPositionRepository
{
    void Add(Position position);
    Task<bool> ExistsActiveWithNameAsync(string name, CancellationToken cancellationToken = default);
}
