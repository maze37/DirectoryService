using CSharpFunctionalExtensions;
using ValueObject = Shared.Base.ValueObject;

namespace DirectoryService.Domain.Department.ValueObjects;

public class PathVO : ValueObject
{
    public const int MinLenght = 3;
    public const int MaxLenght = 150;
    
    public string Value { get; }
    
    private PathVO(string value)
    {
        Value = value;
    }

    public static Result<PathVO> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Result.Failure<PathVO>("Путь не может быть пустым.");
        
        if (value.Length < MinLenght)
            return Result.Failure<PathVO>("Путь названия не может быть меньше 3.");

        if (value.Length > MaxLenght)
            return Result.Failure<PathVO>("Путь названия не может быть больше 150.");
        
        return Result.Success(new PathVO(value));
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
    
    public static implicit operator string(PathVO path) => path.Value;
}