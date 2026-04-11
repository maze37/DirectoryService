using DirectoryService.Contracts.LocationContracts;
using Shared.Core;

namespace DirectoryService.Application.UseCases.LocationCases.CreateLocation;

public record CreateLocationCommand(
        string Name,
        AddressDto Address,
        string Timezone) : ICommand;