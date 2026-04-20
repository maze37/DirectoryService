using DirectoryService.Application.Validation;
using DirectoryService.Domain;
using DirectoryService.Domain.Position.ValueObjects;
using FluentValidation;

namespace DirectoryService.Application.UseCases.PositionCases.CreatePosition;

public class CreatePositionCommandValidator : AbstractValidator<CreatePositionCommand>
{
    public CreatePositionCommandValidator()
    {
        RuleFor(x => x.Request.Name)
            .NotEmpty()
            .WithErrorCode("position.required")
            .WithMessage("Название должности обязательно.")
            .MustBeValueObject(PositionName.Create);
        
        RuleFor(x => x.Request.Description)
            .MaximumLength(LenghtConstants.MAXLENGHT)
            .WithErrorCode("position.description.tooLong")
            .WithMessage("Описание не должно превышать 1000 символов")
            .When(x => x.Request.Description is not null);
        
        RuleFor(x => x.Request.DepartmentIds)
            .NotNull()
            .WithErrorCode("position.departmentIds.required")
            .WithMessage("Список отделов обязателен")
            .NotEmpty()
            .WithErrorCode("position.departmentIds.empty")
            .WithMessage("Список отделов не должен быть пустым")
            .Must(ids => ids.Distinct().Count() == ids.Length)
            .WithErrorCode("position.departmentIds.duplicates")
            .WithMessage("Список отделов не должен содержать дубликаты");
    }
}
