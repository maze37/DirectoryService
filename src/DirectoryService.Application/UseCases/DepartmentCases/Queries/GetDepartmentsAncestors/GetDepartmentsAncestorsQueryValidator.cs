using FluentValidation;

namespace DirectoryService.Application.UseCases.DepartmentCases.Queries.GetDepartmentsAncestors;

public class GetDepartmentsAncestorsQueryValidator : AbstractValidator<GetDepartmentsAncestorsQuery>
{
    public GetDepartmentsAncestorsQueryValidator()
    {
        RuleFor(x => x.DepartmentId)
            .NotEmpty()
            .WithMessage("Айди родительского отдела не может быть пустым");
    }
}