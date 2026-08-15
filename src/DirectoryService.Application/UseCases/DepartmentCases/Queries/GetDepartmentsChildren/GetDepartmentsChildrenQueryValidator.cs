using FluentValidation;

namespace DirectoryService.Application.UseCases.DepartmentCases.Queries.GetDepartmentsChildren;

public class GetDepartmentsChildrenQueryValidator : AbstractValidator<GetDepartmentsChildrenQuery>
{
    public GetDepartmentsChildrenQueryValidator()
    {
        RuleFor(x => x.ParentDepartmentId)
            .NotNull()
            .WithMessage("Айди родительского отдела не может быть пустым");
    }
}