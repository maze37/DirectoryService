using FluentValidation;

namespace DirectoryService.Application.UseCases.DepartmentCases.Queries.GetDepartmentById;

public class GetDepartmentByIdValidator : AbstractValidator<GetDepartmentByIdQuery>
{
    public GetDepartmentByIdValidator()
    {
        RuleFor(x => x.Id)
            .NotNull()
            .WithMessage("Айди отдела не может быть пустым");
    }
}