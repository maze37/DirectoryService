namespace Shared.Exceptions;

public class ValidationException : Exception
{
    public Error Error { get; }

    public ValidationException(Error error) : base(error.Message) 
    {
        Error = error;
    }
    
    public ValidationException() : this("Validation error") { }

    public ValidationException(string message) : base(message) 
    {
        Error = Error.Validation("validation.error", message);
    }

    public ValidationException(string message, Exception innerException)
        : base(message, innerException) 
    {
        Error = Error.Validation("validation.error", message);
    }
}