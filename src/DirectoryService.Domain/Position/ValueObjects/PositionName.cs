using CSharpFunctionalExtensions;
using Shared.Result;
using ValueObject = Shared.Base.ValueObject;

namespace DirectoryService.Domain.Position.ValueObjects;

public class PositionName : ValueObject
{
    public const int MIN_NAME_LENGTH = 3;
    public const int MAX_NAME_LENGHT = 100;
    
    public string Value { get; }
    
    private PositionName(string value)
    {
        Value = value;
    }

    public static Result<PositionName, Error> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return GeneralErrors.ValueIsInvalid("value", "Название позиции не может быть пустым.");

        if (value.Length < MIN_NAME_LENGTH)
            return GeneralErrors.ValueIsInvalid("value.Lenght",$"Название позиции не может быть меньше {MIN_NAME_LENGTH} символов.");

        if (value.Length > MAX_NAME_LENGHT)
            return GeneralErrors.ValueIsInvalid("value.Lenght", $"Название позиции не может быть больше {MAX_NAME_LENGHT} символов.");

        return new PositionName(value);
    }
    
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public static implicit operator string(PositionName name) => name.Value;
}