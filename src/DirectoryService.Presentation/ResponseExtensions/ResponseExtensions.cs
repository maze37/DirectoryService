using Microsoft.AspNetCore.Mvc;
using Shared.Result;

namespace DirectoryService.Presentation.ResponseExtensions;

public static class ResponseExtensions
{
    public static ActionResult ToResponse(this Error error)
    {
        var statusCode = GetStatusCodeFromErrorType(error.Type);

        return new ObjectResult(Envelope.Error(error))
        {
            StatusCode = statusCode,
        };
    }
    
    public static ActionResult ToResponse(this ErrorList errors)
    {
        var firstError = errors.FirstOrDefault();
        var statusCode = firstError is null
            ? StatusCodes.Status500InternalServerError
            : GetStatusCodeFromErrorType(firstError.Type);

        return new ObjectResult(Envelope.Error(errors)) { StatusCode = statusCode };
    }

    private static int GetStatusCodeFromErrorType(ErrorType errorType) =>
        errorType switch
        {
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            ErrorType.Failure => StatusCodes.Status500InternalServerError,
            _ => StatusCodes.Status500InternalServerError
        };
}