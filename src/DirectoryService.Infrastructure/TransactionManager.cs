using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Shared.Core;

namespace DirectoryService.Infrastructure;

/// <inheritdoc/>
public class TransactionManager : ITransactionManager
{
    private readonly AppDbContext _context;
    
    public TransactionManager(AppDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }
    
    public async Task<Result<Unit, Error>> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await _context.SaveChangesAsync(cancellationToken);
            return Unit.Value;
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException pgEx)
        {
            if (pgEx.SqlState == PostgresErrorCodes.UniqueViolation)
                return Error.Conflict(
                    "record.already.exist",
                    "Unique constraint violated",
                    pgEx.ConstraintName);

            return Error.Failure(
                "db.update.failed",
                "Database update failed");
        }
        catch (DbUpdateException)
        {
            return Error.Failure("db.update.failed", "Database update failed");
        }
    }
}
