using DirectoryService.Application.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DirectoryService.Infrastructure.BackgroundServices.Cleanup;

public class CleanupDepartmentsService : BaseCleanupBackgroundService<DepartmentsCleanupOptions>
{
    private readonly IServiceScopeFactory _scopeFactory;

    public CleanupDepartmentsService(
        IServiceScopeFactory scopeFactory,
        IOptions<DepartmentsCleanupOptions> options,
        ILogger<CleanupDepartmentsService> logger)
        : base(options, logger)
    {
        _scopeFactory = scopeFactory;
    }

    protected override string EntityName => "Departments";

    protected override async Task<int> DeleteBatchAsync(
        DateTime olderThanUtc,
        int batchSize,
        CancellationToken cancellationToken)
    {
        // BackgroundService живёт всё время работы приложения (Singleton),
        // а IDepartmentRepository / DbContext — Scoped. Поэтому на каждый
        // вызов создаём свой scope и резолвим репозиторий из него.
        using var scope = _scopeFactory.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IDepartmentRepository>();

        return await repository.DeleteSoftDeletedBatchAsync(olderThanUtc, batchSize, cancellationToken);
    }
}