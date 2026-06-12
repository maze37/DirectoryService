namespace DirectoryService.Contracts.DepartmentContracts;

public record CreateDepartmentRequest(
    string Name, 
    string Slug, 
    Guid? ParentId, 
    Guid[] LocationIds);