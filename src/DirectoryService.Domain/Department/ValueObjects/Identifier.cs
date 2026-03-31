using CSharpFunctionalExtensions;
using ValueObject = Shared.Base.ValueObject;

namespace DirectoryService.Domain.Department.ValueObjects;

public class Identifier : ValueObject
{
    public const int MinLenght = 3;
    public const int MaxLenght = 150;
    
    public string Value { get; }
    
    private Identifier(string value)
    {
        Value = value;
    }

    public static Result<Identifier> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Result.Failure<Identifier>("Путь не может быть пустым.");
        
        if (value.Length < MinLenght)
            return Result.Failure<Identifier>("Путь названия не может быть меньше 3.");

        if (value.Length > MaxLenght)
            return Result.Failure<Identifier>("Путь названия не может быть больше 150.");

        foreach (var c in value)
        {
            if (!char.IsAsciiLetter(c))
                return Result.Failure<Identifier>("В пути могут быть только латинские буквы.");
        }
        
        return Result.Success(new Identifier(value));
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
    
    public static implicit operator string(Identifier identifier) => identifier.Value;
}