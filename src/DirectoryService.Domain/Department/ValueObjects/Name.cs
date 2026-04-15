using CSharpFunctionalExtensions;
using Shared.Result;
using ValueObject = Shared.Base.ValueObject;

namespace DirectoryService.Domain.Department.ValueObjects;

public class Name : ValueObject
{
    public const int MIN_NAME_LENGHT = 3;
    public const int MAX_NAME_LENGHT = 150;
    
    public string Value { get; }
    
    private Name(string value)
    {
        Value = value;
    }

    public static Result<Name, Error> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return GeneralErrors.ValueIsRequired("name");

        if (value.Length < MIN_NAME_LENGHT)
            return GeneralErrors.ValueIsInvalid("name", $"Название не может быть короче {MIN_NAME_LENGHT} символов");

        if (value.Length > MAX_NAME_LENGHT)
            return GeneralErrors.ValueIsInvalid("name", $"Название не может быть длиннее {MAX_NAME_LENGHT} символов");
        
        return new Name(value);
    }
    
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public static implicit operator string(Name name) => name.Value;
}