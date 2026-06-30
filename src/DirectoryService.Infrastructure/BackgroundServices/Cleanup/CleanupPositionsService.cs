using DirectoryService.Application.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DirectoryService.Infrastructure.BackgroundServices.Cleanup;

public class CleanupPositionsService : BaseCleanupBackgroundService<PositionsCleanupOptions>
{
    private readonly IServiceScopeFactory _scopeFactory;

    public CleanupPositionsService(
        IServiceScopeFactory scopeFactory,
        IOptions<PositionsCleanupOptions> options,
        ILogger<CleanupPositionsService> logger)
        : base(options, logger)
    {
        _scopeFactory = scopeFactory;
    }

    protected override string EntityName => "Positions";

    protected override async Task<int> DeleteBatchAsync(
        DateTime olderThanUtc,
        int batchSize,
        CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IPositionRepository>();

        return await repository.DeleteSoftDeletedBatchAsync(olderThanUtc, batchSize, cancellationToken);
    }
}