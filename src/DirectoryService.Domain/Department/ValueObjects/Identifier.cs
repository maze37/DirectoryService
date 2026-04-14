using System.Text.RegularExpressions;
using CSharpFunctionalExtensions;
using Shared.Result;
using ValueObject = Shared.Base.ValueObject;

namespace DirectoryService.Domain.Department.ValueObjects;

public class Identifier : ValueObject
{
    public const int IDENTIFIER_MIN_LENGTH = 3;
    public const int IDENTIFIER_MAX_LENGTH = 150;
    
    public string Value { get; }
    
    private Identifier(string value)
    {
        Value = value;
    }

    public static Result<Identifier, Error> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return GeneralErrors.ValueIsRequired("department identifier");
        }
        
        if (value.Length is > IDENTIFIER_MAX_LENGTH or < IDENTIFIER_MIN_LENGTH)
        {
            return GeneralErrors.ValueIsInvalid("department identifier", "Identifier must be between 3 and 150 characters");
        }

        if (!Regex.IsMatch(value, @"^[a-zA-Z]*$"))
        {
            return GeneralErrors.ValueIsInvalid("department identifier", "Identifier must be in Latin characters");
        }

        return new Identifier(value);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
    
    public static implicit operator string(Identifier identifier) => identifier.Value;
}