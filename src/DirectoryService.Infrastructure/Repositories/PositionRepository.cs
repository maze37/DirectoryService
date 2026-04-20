using DirectoryService.Application.Abstractions;
using DirectoryService.Domain.Position;
using Microsoft.EntityFrameworkCore;

namespace DirectoryService.Infrastructure.Repositories;

public class PositionRepository : IPositionRepository
{
    private readonly AppDbContext _context;

    public PositionRepository(AppDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task AddAsync(Position position, CancellationToken cancellationToken = default)
    {
        await _context.Positions.AddAsync(position, cancellationToken);
    }

    public async Task<bool> ExistsActiveWithNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await _context.Positions
            .AnyAsync(p => p.Name.Value == name && p.IsActive, cancellationToken);
    }
}
