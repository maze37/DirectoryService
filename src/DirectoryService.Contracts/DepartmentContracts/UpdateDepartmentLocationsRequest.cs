namespace DirectoryService.Contracts.DepartmentContracts;

public record UpdateDepartmentLocationsRequest(IReadOnlyList<Guid> LocationIds);