namespace Shared.Result;

public record Envelope
{
    public object? Result { get; }

    public ErrorList? Errors { get; }

    public DateTimeOffset TimeGenerated { get; }

    private Envelope(object? result, ErrorList? errors)
    {
        Result = result;
        Errors = errors;
        TimeGenerated = DateTimeOffset.UtcNow;
    }
  
    public static Envelope Ok(object? result = null) =>
        new(result, null);

    public static Envelope Error(ErrorList errors) =>
        new(null, errors);
    
    public static Envelope Error(Error error) =>
        new(null, error.ToErrorList());
}