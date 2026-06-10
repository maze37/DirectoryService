using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Domain.Position;
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

    public async Task<bool> ExistsActiveWithNameAsync(string name, CancellationToken cancellationToken = default)
    {
        name = name.Trim();

        return await _context.Positions
            .AnyAsync(p => p.Name.Value == name && p.IsActive, cancellationToken);
    }

    public async Task<Result<Position, Error>> GetByIdWithLock(Guid positionId, CancellationToken cancellationToken = default)
    {
        try
        {
            var position = await _context.Positions
                .FromSqlRaw("""
                            SELECT id, description, is_active, created_when, updated_when, name, xmin
                            FROM positions
                            WHERE id = {0}
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
}
