using Shared.Exceptions;
using Shared.Result;

namespace DirectoryService.Presentation.Middlewares;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        _logger.LogError(exception, exception.Message);

        (int statusCode, Error error) = exception switch
        {
            NotFoundException ex => (StatusCodes.Status404NotFound, ex.Error),
            ValidationException ex => (StatusCodes.Status400BadRequest, ex.Error),
            FailureException ex => (StatusCodes.Status500InternalServerError, ex.Error),
            ConflictException ex => (StatusCodes.Status409Conflict, ex.Error),
            _ => (StatusCodes.Status500InternalServerError, Error.Failure("server.internal", exception.Message))
        };

        var envelope = Envelope.Error(error);
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;

        await context.Response.WriteAsJsonAsync(envelope);
    }
}

public static class ExceptionMiddlewareExtensions
{
    public static IApplicationBuilder UseExceptionMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<ExceptionMiddleware>();
    }
}