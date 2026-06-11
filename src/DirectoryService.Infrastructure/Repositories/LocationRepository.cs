using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Domain.Location;
using Microsoft.EntityFrameworkCore;
using Shared.Result;

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

    public async Task<Result<Location, Error>> GetByIdWithLock(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var location = await _context.Locations
                .FromSqlRaw("""
                                SELECT id, is_active, created_when, updated_when, address_building,
                                       address_city, address_country, address_office, address_postal_code,
                                       address_street, name, timezone, xmin
                                FROM locations
                                WHERE id = {0}
                                FOR UPDATE
                            """, id)
                .FirstOrDefaultAsync(cancellationToken);

            if (location is null)
                return Errors.General.NotFound(name: "location");

            return location;
        }
        catch (Exception ex)
        {
            return Error.Failure("location.get.failed", ex.Message);
        }
    }

    public async Task<bool> HasDepartmentLinksAsync(Guid id, CancellationToken cancellationToken)
    {
        var hasLinks = await _context.DepartmentLocations
            .AnyAsync(x => x.LocationId == id);

        return hasLinks;
    }
    
    public void Remove(Location location)
    {
        _context.Locations.Remove(location);
    }
}