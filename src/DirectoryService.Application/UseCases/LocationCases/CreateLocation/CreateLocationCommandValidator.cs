using DirectoryService.Application.Validation;
using DirectoryService.Domain.Location.ValueObjects;
using FluentValidation;
using Shared.Result;

namespace DirectoryService.Application.UseCases.LocationCases.CreateLocation;

public class CreateLocationCommandValidator : AbstractValidator<CreateLocationCommand>
{
    public CreateLocationCommandValidator()
    {
        RuleFor(x => x.Request.Name)
            .NotEmpty()
                .WithErrorCode("location.name.required")
                .WithMessage("Название локации обязательно")
            .MustBeValueObject(LocationName.Create);

        RuleFor(x => x.Request.Address)
            .NotNull()
                .WithErrorCode("location.address.required")
                .WithMessage("Адрес обязателен");

        When(x => x.Request.Address is not null, () =>
        {
            RuleFor(x => x.Request.Address)
                .MustBeValueObject(a => Address.Create(
                    a.Country, a.City, a.Street,
                    a.Building, a.Office, a.PostalCode));
        });
        
        RuleFor(x => x.Request.Timezone)
            .NotEmpty()
            .WithError(GeneralErrors.ValueIsInvalid("Timezone", "value is empty"));
    }
}