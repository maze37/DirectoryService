using DirectoryService.Contracts.DepartmentContracts;
using Core.Abstractions;

namespace DirectoryService.Application.UseCases.DepartmentCases.Commands.UpdateDepartmentLocations;

public record UpdateDepartmentLocationsCommand(
    Guid DepartmentId, 
    UpdateDepartmentLocationsRequest Request) : ICommand;