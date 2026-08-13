using FluentValidation;

namespace DirectoryService.Application.UseCases.PositionCases.Commands.RestorePosition;

public class RestorePositionCommandValidator : AbstractValidator<RestorePositionCommand>
{
    public RestorePositionCommandValidator()
    {
        RuleFor(x => x.PositionId)
            .NotEmpty()
            .WithErrorCode("restore.position.positionId.required")
            .WithMessage("Айди должности обязателен.");
    }
}