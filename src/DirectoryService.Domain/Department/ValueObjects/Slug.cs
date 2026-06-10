using System.Text.RegularExpressions;
using CSharpFunctionalExtensions;
using Shared.Result;
using ValueObject = Shared.Base.ValueObject;

namespace DirectoryService.Domain.Department.ValueObjects;

public class Slug : ValueObject
{
    public string Value { get; }

    public static Slug From(string value) => new Slug(value);
    
    private Slug(string value)
    {
        Value = value;
    }

    public static Result<Slug, Error> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return GeneralErrors.ValueIsRequired("department Slug");
        }

        if (!Regex.IsMatch(value, @"^[a-zA-Z]*$"))
        {
            return GeneralErrors.ValueIsInvalid("department Slug", "Slug must be in Latin characters");
        }

        return new Slug(value);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
    
    public static implicit operator string(Slug slug) => slug.Value;
}