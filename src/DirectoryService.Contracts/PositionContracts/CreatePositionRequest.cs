namespace DirectoryService.Contracts.PositionContracts;

public record CreatePositionRequest(
    string Name,
    string? Description,
    Guid[] DepartmentIds);
