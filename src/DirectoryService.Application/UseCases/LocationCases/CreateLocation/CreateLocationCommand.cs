using Shared.Core;

namespace DirectoryService.Application.UseCases.LocationCases.CreateLocation;

public record CreateLocationCommand(
        string Name,
        string Country,
        string City,
        string Street,
        string Building,
        string? Office,
        string? PostalCode,
        string Timezone) : ICommand;