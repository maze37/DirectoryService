using CSharpFunctionalExtensions;
using ValueObject = Shared.Base.ValueObject;

namespace DirectoryService.Domain.Position.ValueObjects;

public class PositionName : ValueObject
{
    public const int MinLength = 3;
    public const int MaxLength = 100;
    
    public string Value { get; }
    
    private PositionName(string value)
    {
        Value = value;
    }

    public static Result<PositionName> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Result.Failure<PositionName>("Название позиции не может быть пустым.");

        if (value.Length < MinLength)
            return Result.Failure<PositionName>($"Название позиции не может быть меньше {MinLength} символов.");

        if (value.Length > MaxLength)
            return Result.Failure<PositionName>($"Название позиции не может быть больше {MaxLength} символов.");
        
        return Result.Success(new PositionName(value));
    }
    
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public static implicit operator string(PositionName name) => name.Value;
}