using DirectoryService.Domain.Location;

namespace DirectoryService.Application.Abstractions;

public interface ILocationRepository
{
    Task AddAsync(Location location, CancellationToken cancellationToken = default);
    Task<bool> AllExistAsync(IReadOnlyList<Guid> locationIds, CancellationToken cancellationToken = default);
}