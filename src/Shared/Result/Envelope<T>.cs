namespace Shared.Result;

public record Envelope<T>
{
    public T? Result { get; init; }
    public ErrorList? Errors { get; init; }
    public DateTimeOffset TimeGenerated { get; init; }
}