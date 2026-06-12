using FluentValidation;

namespace DirectoryService.Application.UseCases.LocationCases.Commands.DeleteLocation;

public class DeleteLocationValidator : AbstractValidator<DeleteLocationCommand>
{
    public DeleteLocationValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}