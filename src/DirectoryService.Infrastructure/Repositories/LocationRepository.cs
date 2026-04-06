using DirectoryService.Application.Abstractions;
using DirectoryService.Domain.Location;

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
        try
        {
            await _context.Locations.AddAsync(location, cancellationToken);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка в доменном слое {ex.Message}");
        }
    }
}