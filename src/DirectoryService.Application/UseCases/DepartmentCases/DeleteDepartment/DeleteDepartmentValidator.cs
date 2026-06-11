using FluentValidation;

namespace DirectoryService.Application.UseCases.DepartmentCases.DeleteDepartment;

public class DeleteDepartmentValidator : AbstractValidator<DeleteDepartmentCommand>
{
    public DeleteDepartmentValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}