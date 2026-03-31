using CSharpFunctionalExtensions;
using ValueObject = Shared.Base.ValueObject;

namespace DirectoryService.Domain.Position.ValueObjects;

public class Description : ValueObject
{
    public const int MaxLength = 1000;
    
    public string? Value { get; }
    
    private Description(string? value)
    {
        Value = value;
    }

    public static Result<Description> Create(string? value)
    {
        if (value == null)
            return Result.Success(new Description(null));
        
        if (string.IsNullOrWhiteSpace(value))
            return Result.Success(new Description(null));
        
        value = value.Trim();
        
        if (value.Length > MaxLength)
            return Result.Failure<Description>($"Описание не может быть больше {MaxLength} символов.");
        
        return Result.Success(new Description(value));
    }
    
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value ?? string.Empty;
    }

    public static implicit operator string?(Description description) => description.Value;
}