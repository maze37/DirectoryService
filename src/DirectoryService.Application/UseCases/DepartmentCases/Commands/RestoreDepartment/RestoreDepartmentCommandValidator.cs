using FluentValidation;

namespace DirectoryService.Application.UseCases.DepartmentCases.Commands.RestoreDepartment;

public class RestoreDepartmentCommandValidator : AbstractValidator<RestoreDepartmentCommand>
{
    public RestoreDepartmentCommandValidator()
    {
        RuleFor(x => x.DepartmentId)
            .NotEmpty()
            .WithErrorCode("restore.department.departmentId.required")
            .WithMessage("Айди подразделения обязателен.");
    }
}