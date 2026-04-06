using DirectoryService.Application.Abstractions;

namespace DirectoryService.Infrastructure;

/// <inheritdoc/>
public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;
    
    public UnitOfWork(AppDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }
    
    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken)
            .ConfigureAwait(false);
    }
}