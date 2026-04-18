using FluentValidation.Results;
using Shared.Result;

namespace DirectoryService.Application.Validation;

public static class ValidationExtensions
{
    public static Error ToError(this ValidationResult validationResult)
    {
        var first = validationResult.Errors.First();
        return Error.Validation(
            code: first.ErrorCode,
            message: first.ErrorMessage,
            invalidField: first.PropertyName);
    }
}