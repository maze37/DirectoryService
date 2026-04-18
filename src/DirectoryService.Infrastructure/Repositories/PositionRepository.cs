using DirectoryService.Application.Abstractions;
using DirectoryService.Domain.Position;

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
}