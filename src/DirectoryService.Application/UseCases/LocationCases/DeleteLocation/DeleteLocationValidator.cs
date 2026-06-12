using FluentValidation;

namespace DirectoryService.Application.UseCases.LocationCases.DeleteLocation;

public class DeleteLocationValidator : AbstractValidator<DeleteLocationCommand>
{
    public DeleteLocationValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}