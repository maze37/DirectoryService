using DirectoryService.Contracts.LocationContracts;
using Shared.Core;

namespace DirectoryService.Application.UseCases.LocationCases.Commands.CreateLocation;

public record CreateLocationCommand(
        CreateLocationRequest Request) : ICommand;