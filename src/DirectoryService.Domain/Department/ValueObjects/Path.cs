using CSharpFunctionalExtensions;
using ValueObject = Shared.Base.ValueObject;

namespace DirectoryService.Domain.Department.ValueObjects;

public class Path : ValueObject
{
    public const char SEPARATOR = '.';
    
    public string Value { get; }
    
    public static Path From(string value) => new Path(value);
    
    private Path(string value)
    {
        Value = value;
    }

    public static Path CreateParent(Slug slug)
    {
        return new Path(slug.Value);
    }
    
    public Path CreateChild(Slug childSlug)
    {
        return new Path(Value + SEPARATOR + childSlug.Value);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
    
    public static implicit operator string(Path path) => path.Value;
}