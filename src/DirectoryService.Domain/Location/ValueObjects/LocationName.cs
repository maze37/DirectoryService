using CSharpFunctionalExtensions;
using ValueObject = Shared.Base.ValueObject;

namespace DirectoryService.Domain.Location.ValueObjects;

public class LocationName : ValueObject
{
    public const int MinLength = 3;
    public const int MaxLength = 120;
    
    public string Value { get; }
    
    private LocationName(string value)
    {
        Value = value;
    }

    public static Result<LocationName> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Result.Failure<LocationName>("Название локации не может быть пустым.");

        if (value.Length < MinLength)
            return Result.Failure<LocationName>($"Название локации не может быть меньше {MinLength} символов.");

        if (value.Length > MaxLength)
            return Result.Failure<LocationName>($"Название локации не может быть больше {MaxLength} символов.");
        
        return Result.Success(new LocationName(value));
    }
    
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public static implicit operator string(LocationName name) => name.Value;
}