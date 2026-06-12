using FluentValidation;

namespace DirectoryService.Application.UseCases.LocationCases.Queries.GetLocationById;

public class GetLocationByIdValidator : AbstractValidator<GetLocationByIdQuery>
{
    public GetLocationByIdValidator()
    {
        RuleFor(x => x.Id).NotNull();
    }
}