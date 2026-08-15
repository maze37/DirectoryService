namespace DirectoryService.Contracts.DepartmentContracts;

public record DepartmentTreeItemDto(
    Guid Id,
    string Name, 
    string Slug,
    string Path,
    int Depth,
    bool HasChildren,
    int ChildrenCount);