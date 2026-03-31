using CSharpFunctionalExtensions;
using ValueObject = Shared.Base.ValueObject;

namespace DirectoryService.Domain.Department.ValueObjects;

public class Depth : ValueObject
{
    public short Value { get; }

    private Depth(short value)
    {
        Value = value;
    }

    public static Result<Depth> Create(short value)
    {
        if (value < 0)
            return Result.Failure<Depth>("Глубина подразделения не может быть меньше 0,");
        
        return Result.Success(new Depth(value));
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}