using FluentValidation;

namespace DirectoryService.Application.UseCases.PositionCases.DeletePosition;

public class DeletePositionValidator : AbstractValidator<DeletePositionCommand>
{
    public DeletePositionValidator()
    {
        RuleFor(id => id.Id).NotEmpty();
    }
}