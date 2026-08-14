using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DirectoryService.Infrastructure.BackgroundServices.Cleanup;

public abstract class BaseCleanupBackgroundService<TOptions> : BackgroundService
    where TOptions : CleanupServiceOptions
{
    private readonly TOptions _options;
    private readonly ILogger _logger;

    protected BaseCleanupBackgroundService(
        IOptions<TOptions> options,
        ILogger logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    /// <summary>
    /// Имя сущности — только для логов.
    /// </summary>
    protected abstract string EntityName { get; }

    /// <summary>
    /// Удаляет один батч устаревших soft-deleted записей.
    /// Возвращает количество физически удалённых строк.
    /// </summary>
    protected abstract Task<int> DeleteBatchAsync(
        DateTime olderThanUtc,
        int batchSize,
        CancellationToken cancellationToken);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_options.Enabled)
        {
            _logger.LogInformation("Cleanup для {Entity} отключён в конфигурации", EntityName);
            return;
        }

        // Даём приложению время прогреться перед первым прогоном
        await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await RunCleanupCycleAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                // Нормальное завершение при остановке хоста — не логируем как ошибку
                break;
            }
            catch (Exception ex)
            {
                // Ключевое требование: ошибка одного прогона не должна ронять приложение
                _logger.LogError(ex, "Ошибка при очистке {Entity}", EntityName);
            }

            try
            {
                await Task.Delay(_options.Interval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }

    private async Task RunCleanupCycleAsync(CancellationToken cancellationToken)
    {
        var olderThanUtc = DateTime.UtcNow - _options.RetentionPeriod;
        var totalDeleted = 0;

        while (!cancellationToken.IsCancellationRequested)
        {
            var deletedInBatch = await DeleteBatchAsync(olderThanUtc, _options.BatchSize, cancellationToken);
            totalDeleted += deletedInBatch;

            if (deletedInBatch < _options.BatchSize)
                break; // батч неполный — кандидатов больше нет, цикл окончен

            await Task.Delay(_options.DelayBetweenBatches, cancellationToken);
        }

        if (totalDeleted > 0)
            _logger.LogInformation("Cleanup {Entity}: удалено {Count} записей", EntityName, totalDeleted);
    }
}