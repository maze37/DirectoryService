using CSharpFunctionalExtensions;
using Shared.Result;
using ValueObject = Shared.Base.ValueObject;

namespace DirectoryService.Domain.Location.ValueObjects;

public class LocationName : ValueObject
{
    public const int MIN_NAME_LENGHT = 3;
    public const int MAX_NAME_LENGHT = 120;
    
    public string Value { get; }
    
    private LocationName(string value)
    {
        Value = value;
    }

    public static Result<LocationName, Error> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return GeneralErrors.ValueIsRequired("location name");

        if (value.Length < MIN_NAME_LENGHT)
            return GeneralErrors.ValueIsInvalid("location name", $"Название локации не может быть меньше {MIN_NAME_LENGHT} символов.");

        if (value.Length > MAX_NAME_LENGHT)
            return GeneralErrors.ValueIsInvalid("location name", $"Название локации не может быть больше {MAX_NAME_LENGHT} символов.");
        
        return new LocationName(value);
    }
    
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public static implicit operator string(LocationName name) => name.Value;
}