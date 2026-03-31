using CSharpFunctionalExtensions;
using ValueObject = Shared.Base.ValueObject;

namespace DirectoryService.Domain.Department.ValueObjects;

public class Name : ValueObject
{
    public const int MinLenght = 3;
    public const int MaxLenght = 150;
    
    public string Value { get; }
    
    private Name(string value)
    {
        Value = value;
    }

    public static Result<Name> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Result.Failure<Name>("Название подразделения не может быть пустым.");

        if (value.Length < MinLenght)
            return Result.Failure<Name>("Длина названия не может быть меньше 3.");

        if (value.Length > MaxLenght)
            return Result.Failure<Name>("Длина названия не может быть больше 150.");
        
        return Result.Success(new Name(value));
    }
    
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public static implicit operator string(Name name) => name.Value;
}