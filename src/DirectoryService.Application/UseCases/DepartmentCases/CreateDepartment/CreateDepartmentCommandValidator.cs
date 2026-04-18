using DirectoryService.Application.Validation;
using DirectoryService.Domain.Department.ValueObjects;
using DirectoryService.Domain.Location.ValueObjects;
using FluentValidation;

namespace DirectoryService.Application.UseCases.DepartmentCases.CreateDepartment;

public class CreateDepartmentCommandValidator : AbstractValidator<CreateDepartmentCommand>
{
    public CreateDepartmentCommandValidator()
    {
        RuleFor(x => x.Request.Name)
            .NotEmpty()
                .WithErrorCode("department.departmentName.required")
                .WithMessage("Название отдела обязательно")
            .MustBeValueObject(DepartmentName.Create);

        RuleFor(x => x.Request.Identifier)
            .NotEmpty()
                .WithErrorCode("department.departmentIdentifier.required")
                .WithMessage("Идентификатор отдела обязательна")
            .MustBeValueObject(Identifier.Create);

        RuleFor(x => x.Request.LocationIds)
            .NotNull()
            .WithErrorCode("department.locationIds.required")
            .WithMessage("Список локаций обязателен")
            .NotEmpty()
            .WithErrorCode("department.locationIds.empty")
            .WithMessage("Список локаций не должен быть пустым")
            .Must(ids => ids.Distinct().Count() == ids.Length)
            .WithErrorCode("department.locationIds.duplicates")
            .WithMessage("Список локаций не должен содержать дубликаты");
    }
}