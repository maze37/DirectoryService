using CSharpFunctionalExtensions;
using ValueObject = Shared.Base.ValueObject;

namespace DirectoryService.Domain.Location.ValueObjects;

public class Address : ValueObject
{
    public string Value { get; }
    
    private Address(string value)
    {
        Value = value;
    }

    public static Result<Address> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Result.Failure<Address>("Адрес не может быть пустым.");
            
        if (value.Length < 5)
            return Result.Failure<Address>("Адрес слишком короткий.");
            
        if (value.Length > 500)
            return Result.Failure<Address>("Адрес не может быть длиннее 500 символов.");
        
        return Result.Success(new Address(value.Trim()));
    }
    
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public static implicit operator string(Address address) => address.Value;
}