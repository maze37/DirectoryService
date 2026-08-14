using FluentValidation;

namespace DirectoryService.Application.UseCases.LocationCases.Commands.RestoreLocation;

public class RestoreLocationCommandValidator : AbstractValidator<RestoreLocationCommand>
{
    public RestoreLocationCommandValidator()
    {
        RuleFor(x => x.LocationId)
            .NotEmpty()
            .WithErrorCode("restore.location.locationId.required")
            .WithMessage("Айди локации обязателен.");
    }
}