using DirectoryService.Application.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DirectoryService.Infrastructure.BackgroundServices.Cleanup;

public class CleanupLocationsService : BaseCleanupBackgroundService<LocationsCleanupOptions>
{
    private readonly IServiceScopeFactory _scopeFactory;

    public CleanupLocationsService(
        IServiceScopeFactory scopeFactory,
        IOptions<LocationsCleanupOptions> options,
        ILogger<CleanupLocationsService> logger)
        : base(options, logger)
    {
        _scopeFactory = scopeFactory;
    }

    protected override string EntityName => "Locations";

    protected override async Task<int> DeleteBatchAsync(
        DateTime olderThanUtc,
        int batchSize,
        CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<ILocationRepository>();

        return await repository.DeleteSoftDeletedBatchAsync(olderThanUtc, batchSize, cancellationToken);
    }
}