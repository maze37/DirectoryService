using DirectoryService.Domain.Location;

namespace DirectoryService.Application.Abstractions;

public interface ILocationRepository
{
    void Add(Location location);
    Task<bool> AllExistAsync(IReadOnlyList<Guid> locationIds, CancellationToken cancellationToken = default);
}