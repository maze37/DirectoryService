using FluentValidation;

namespace DirectoryService.Application.UseCases.PositionCases.RenamePosition;

public class RenamePositionValidator : AbstractValidator<RenamePositionCommand>
{
    public RenamePositionValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Id позиции обязателен");
        
        RuleFor(x => x.Request.Name)
            .NotEmpty()
            .WithMessage("Название позиции не может быть пустым")
            .MaximumLength(255)
            .WithMessage("Название позиции не должно превышать 255 символов")
            .MinimumLength(2)
            .WithMessage("Название позиции должно содержать минимум 2 символа")
            .Matches("^[a-zA-Zа-яА-Я0-9\\s\\-_\\.]+$")
            .WithMessage("Название позиции содержит недопустимые символы");
        
        // Дополнительная проверка: имя не должно состоять только из пробелов
        RuleFor(x => x.Request.Name)
            .Must(name => !string.IsNullOrWhiteSpace(name))
            .WithMessage("Название позиции не может состоять только из пробелов");
    }
}