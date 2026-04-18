using DirectoryService.Application.Abstractions;
using DirectoryService.Domain.Location;
using DirectoryService.Infrastructure.Configurations;
using Microsoft.EntityFrameworkCore;

namespace DirectoryService.Infrastructure.Repositories;

public class LocationRepository : ILocationRepository
{
    private readonly AppDbContext _context;
    
    public LocationRepository(AppDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task AddAsync(
        Location location, 
        CancellationToken cancellationToken = default)
    {
        await _context.Locations.AddAsync(location, cancellationToken);
    }

    public async Task<bool> AllExistAsync(
        Guid[] locationIds, 
        CancellationToken cancellationToken = default)
    {
        var existingCount = await _context.Locations
            .Where(l => locationIds.Contains(l.Id))
            .CountAsync(cancellationToken);
        
        return existingCount == locationIds.Length;
    }
}