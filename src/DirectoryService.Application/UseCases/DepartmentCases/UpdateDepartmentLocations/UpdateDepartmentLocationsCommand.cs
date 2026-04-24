using DirectoryService.Contracts.DepartmentContracts;
using Shared.Core;

namespace DirectoryService.Application.UseCases.DepartmentCases.UpdateDepartmentLocations;

public record UpdateDepartmentLocationsCommand(
    Guid DepartmentId, 
    UpdateDepartmentLocationsRequest Request) : ICommand;