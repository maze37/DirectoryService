using DirectoryService.Contracts.LocationContracts;
using Shared.Core;

namespace DirectoryService.Application.UseCases.LocationCases.CreateLocation;

public record CreateLocationCommand(
        CreateLocationRequest Request) : ICommand;