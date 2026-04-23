using FluentValidation;

namespace DirectoryService.Application.UseCases.DepartmentCases.UpdateDepartmentLocations;

public class UpdateDepartmentLocationsValidator : AbstractValidator<UpdateDepartmentLocationsCommand>
{
    public UpdateDepartmentLocationsValidator()
    {
        RuleFor(x => x.DepartmentId)
            .NotEmpty().WithMessage("departmentId обязателен");

        RuleFor(x => x.Request.LocationIds)
            .NotEmpty().WithMessage("locationIds не может быть пустым")
            .Must(ids => ids.Distinct().Count() == ids.Count)
            .WithMessage("locationIds содержит дубликаты");
    }
}