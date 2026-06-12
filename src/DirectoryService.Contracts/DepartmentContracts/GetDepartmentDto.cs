namespace DirectoryService.Contracts.DepartmentContracts;

public record GetDepartmentDto
{
    public Guid Id { get; init; }
    public string DepartmentName { get; init; } = null!;
    public string Slug { get; init; } = null!;
    public Guid? ParentId { get; init; }
    public string Path { get; init; } = null!;
    public int Depth { get; init; }
    public int ChildrenCount { get; init; }
    public bool IsActive { get; init; }
    public DateTimeOffset CreatedWhen { get; init; }
    public DateTimeOffset UpdatedWhen { get; init; }
}