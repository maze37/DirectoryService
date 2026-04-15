namespace Shared.Exceptions;

public class BadRequestException : Exception
{
    public Error Error { get; } = null!;

    public BadRequestException(Error error) : base(error.Message) => Error = error;
    
    public BadRequestException() { }
    
    public BadRequestException(string message) : base(message) { }
    
    public BadRequestException(string message, Exception innerException) 
        : base(message, innerException) { }
}