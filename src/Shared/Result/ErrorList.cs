using System.Collections;

namespace Shared.Result;

public class ErrorList : IEnumerable<Error>
{
    private readonly List<Error> _errors;

    public ErrorList(IEnumerable<Error> errors)
    {
        _errors = [..errors];
    }
    
    public IEnumerator<Error> GetEnumerator()
    {
        return _errors.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
    
#pragma warning disable CA1002, CA2225
    public static implicit operator ErrorList(List<Error> errors)
        => new(errors);
    
    public static implicit operator ErrorList(Error error)
        => new([error]);
    
#pragma warning restore CA1002, CA2225
}