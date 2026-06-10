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
}
