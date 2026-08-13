using CSharpFunctionalExtensions;
using Dapper;
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

    public async Task<Result<Location, Error>> GetByIdWithLock(
        Guid id, 
        CancellationToken cancellationToken)
    {
        try
        {
            var dbConn = _context.Database.GetDbConnection();
            await dbConn.ExecuteAsync(new CommandDefinition(
                "SELECT 1 FROM locations WHERE id = @id AND is_deleted = false FOR UPDATE",
                new { id },
                cancellationToken: cancellationToken));

            var location = await _context.Locations
                .FirstOrDefaultAsync(l => l.Id == id, cancellationToken);

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
            .AnyAsync(x => x.LocationId == id, cancellationToken);

        return hasLinks;
    }
    
    public void Remove(Location location)
    {
        _context.Locations.Remove(location);
    }
    
    public async Task<int> DeleteSoftDeletedBatchAsync(
        DateTimeOffset olderThanUtc,
        int batchSize,
        CancellationToken cancellationToken)
    {
        const string sql = """
                           DELETE FROM locations
                           WHERE id IN (
                               SELECT id FROM locations
                               WHERE is_deleted = true AND deleted_when < @OlderThanUtc
                               ORDER BY deleted_when
                               LIMIT @BatchSize
                           )
                           """;

        var connection = _context.Database.GetDbConnection();
        
        if (connection.State != System.Data.ConnectionState.Open)
            await _context.Database.OpenConnectionAsync(cancellationToken);

        var rowsAffected = await connection.ExecuteAsync(
            new CommandDefinition(
                sql,
                new { OlderThanUtc = olderThanUtc, BatchSize = batchSize },
                cancellationToken: cancellationToken));

        return rowsAffected;
    }
    
    /*
    public async Task<int> DeleteSoftDeletedBatchAsync(
        DateTime olderThanUtc,
        int batchSize,
        CancellationToken cancellationToken)
    {
        var idsToDelete = await _context.Locations
            .IgnoreQueryFilters() // иначе глобальный фильтр is_deleted=false скроет сами кандидаты на удаление
            .Where(d => d.IsDeleted && d.DeletedWhen < olderThanUtc)
            .OrderBy(d => d.DeletedAt)
            .Take(batchSize)
            .Select(d => d.Id)
            .ToListAsync(cancellationToken);

        if (idsToDelete.Count == 0)
            return 0;

        var rowsAffected = await _context.Locations
            .IgnoreQueryFilters()
            .Where(d => idsToDelete.Contains(d.Id))
            .ExecuteDeleteAsync(cancellationToken);

        return rowsAffected;
    }
    */
}