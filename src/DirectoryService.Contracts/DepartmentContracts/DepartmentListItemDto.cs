namespace DirectoryService.Contracts.DepartmentContracts;

public record DepartmentListItemDto(
    string Name, 
    string Slug, 
    string Path, 
    DateTimeOffset CreatedWhen);