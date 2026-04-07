using DirectoryService.Application.Abstractions;

namespace DirectoryService.Infrastructure;

/// <inheritdoc/>
public class TransactionManager : IUnitOfWork
{
    private readonly AppDbContext _context;
    
    public TransactionManager(AppDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }
    
    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken)
            .ConfigureAwait(false);
    }
}