using FluentValidation;

namespace DirectoryService.Application.UseCases.DepartmentCases.MoveDepartment;

public class MoveDepartmentCommandValidator : AbstractValidator<MoveDepartmentCommand>
{
    public MoveDepartmentCommandValidator()
    {
        RuleFor(x => x.DepartmentId)
            .NotNull()
            .WithErrorCode("move.department.departmentId.required")
            .WithMessage("Айди подразделения обязателен.")
            .NotEmpty();
    }
}