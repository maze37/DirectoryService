using FluentValidation;

namespace DirectoryService.Application.UseCases.DepartmentCases.Commands.DeleteDepartment;

public class DeleteDepartmentValidator : AbstractValidator<DeleteDepartmentCommand>
{
    public DeleteDepartmentValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}