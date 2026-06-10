using DirectoryService.Application.Abstractions;
using DirectoryService.Domain.Location;
using Microsoft.EntityFrameworkCore;

namespace DirectoryService.Infrastructure.Repositories;

public class LocationRepository : ILocationRepository
{
    private readonly AppDbContext _context;
    
    public LocationRepository(AppDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public void Add(Location location)
    {
        _context.Locations.Add(location);
    }

    public async Task<bool> AllExistAsync(
        IReadOnlyList<Guid> locationIds, 
        CancellationToken cancellationToken = default)
    {
        var existingCount = await _context.Locations
            .Where(l => locationIds.Contains(l.Id))
            .CountAsync(cancellationToken);
        
        return existingCount == locationIds.Count;
    }
}