using CSharpFunctionalExtensions;
using ValueObject = Shared.Base.ValueObject;

namespace DirectoryService.Domain.Department.ValueObjects;

public class Path : ValueObject
{
    public const char SEPARATOR = '/';
    
    public string Value { get; }
    
    private Path(string value)
    {
        Value = value;
    }

    public static Path CreateParent(Identifier identifier)
    {
        return new Path(identifier.Value);
    }
    
    public Path CreateChild(Identifier childIdentifier)
    {
        return new Path(Value + SEPARATOR + childIdentifier.Value);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
    
    public static implicit operator string(Path path) => path.Value;
}