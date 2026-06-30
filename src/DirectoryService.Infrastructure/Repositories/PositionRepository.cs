using CSharpFunctionalExtensions;
using Dapper;
using DirectoryService.Application.Abstractions;
using DirectoryService.Domain.Position;
using DirectoryService.Domain.Position.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Shared.Result;

namespace DirectoryService.Infrastructure.Repositories;

public class PositionRepository : IPositionRepository
{
    private readonly AppDbContext _context;

    public PositionRepository(AppDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public void Add(Position position)
    {
        _context.Positions.Add(position);
    }

    public void Remove(Position position)
    {
        _context.Positions.Remove(position);
    }

    public async Task<bool> HasDepartmentLinksAsync(Guid id, CancellationToken cancellationToken)
    {
        var hasLinks = await _context.DepartmentPositions
            .AnyAsync(i => i.PositionId == id, cancellationToken);

        return hasLinks;
    }

    public async Task<bool> ExistsActiveWithNameAsync(string name, CancellationToken cancellationToken = default)
    {
        var nameValue = PositionName.From(name);

        return await _context.Positions
            .AnyAsync(p => p.Name == nameValue && p.IsActive, cancellationToken);
    }

    public async Task<Result<Position, Error>> GetByIdWithLock(Guid positionId, CancellationToken cancellationToken = default)
    {
        try
        {
            var position = await _context.Positions
                .FromSqlRaw("""
                            SELECT id, description, is_active, created_when, updated_when, name, xmin
                            FROM positions
                            WHERE id = {0} AND is_deleted = false
                            FOR UPDATE
                            """, positionId)
                .FirstOrDefaultAsync(cancellationToken);

            if (position is null)
                return Errors.General.NotFound(name: "position");

            return position;
        }
        catch (Exception ex)
        {
            return Error.Failure("position.get.failed", ex.Message);
        }
    }

    public void Update(Position position)
    {
        _context.Positions.Update(position);
    }
    
    public async Task<int> DeleteSoftDeletedBatchAsync(
        DateTime olderThanUtc,
        int batchSize,
        CancellationToken cancellationToken)
    {
        const string sql = """
                           DELETE FROM positions
                           WHERE id IN (
                               SELECT id FROM positions
                               WHERE is_deleted = true AND deleted_when < @OlderThanUtc
                               ORDER BY deleted_when
                               LIMIT @BatchSize
                           )
                           """;

        var connection = _context.Database.GetDbConnection();

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
        var idsToDelete = await _context.Positions
            .IgnoreQueryFilters() // иначе глобальный фильтр is_deleted=false скроет сами кандидаты на удаление
            .Where(d => d.IsDeleted && d.DeletedWhen < olderThanUtc)
            .OrderBy(d => d.DeletedAt)
            .Take(batchSize)
            .Select(d => d.Id)
            .ToListAsync(cancellationToken);

        if (idsToDelete.Count == 0)
            return 0;

        var rowsAffected = await _context.Positions
            .IgnoreQueryFilters()
            .Where(d => idsToDelete.Contains(d.Id))
            .ExecuteDeleteAsync(cancellationToken);

        return rowsAffected;
    }
    */
}