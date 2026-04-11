namespace Shared.Result;

public static class GeneralErrors
{
    public static Error ValueIsInvalid(string? field, string message)
    {
        var label = field ?? "value";
        return Error.Validation($"{label} is invalid", $"{message}", $"{field}");
    }
}