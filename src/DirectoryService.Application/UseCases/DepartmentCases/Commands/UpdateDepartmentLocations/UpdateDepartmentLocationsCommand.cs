using DirectoryService.Contracts.DepartmentContracts;
using Shared.Core;

namespace DirectoryService.Application.UseCases.DepartmentCases.Commands.UpdateDepartmentLocations;

public record UpdateDepartmentLocationsCommand(
    Guid DepartmentId, 
    UpdateDepartmentLocationsRequest Request) : ICommand;