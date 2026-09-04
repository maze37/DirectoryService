using Core.Abstractions;
using DirectoryService.Contracts.LocationContracts;

namespace DirectoryService.Application.UseCases.LocationCases.Commands.CreateLocation;

public record CreateLocationCommand(
        CreateLocationRequest Request) : ICommand;