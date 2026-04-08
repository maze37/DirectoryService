using DirectoryService.Application.Abstractions;

namespace DirectoryService.Infrastructure;

/// <inheritdoc/>
public class DateTimeProvider : IDateTimeProvider
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}