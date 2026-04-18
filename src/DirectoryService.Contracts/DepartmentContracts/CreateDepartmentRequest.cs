namespace DirectoryService.Contracts.DepartmentContracts;

public record CreateDepartmentRequest(
    string Name, 
    string Identifier, 
    Guid? ParentId, 
    Guid[] LocationIds);