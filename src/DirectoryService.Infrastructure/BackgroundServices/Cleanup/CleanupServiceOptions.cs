namespace DirectoryService.Infrastructure.BackgroundServices.Cleanup;

public class CleanupServiceOptions
{
    public bool Enabled { get; init; } = true;

    /// <summary>Как часто запускать прогон очистки.</summary>
    public TimeSpan Interval { get; init; } = TimeSpan.FromHours(1);

    /// <summary>Через сколько после удаления запись можно физически удалять.</summary>
    public TimeSpan RetentionPeriod { get; init; } = TimeSpan.FromDays(30);

    /// <summary>Сколько строк удалять за один проход цикла.</summary>
    public int BatchSize { get; init; } = 200;

    /// <summary>Пауза между батчами внутри одного прогона (разгрузка БД).</summary>
    public TimeSpan DelayBetweenBatches { get; init; } = TimeSpan.FromMilliseconds(200);
}