namespace DirectoryService.Contracts.LocationContracts;

public record LocationListItemDto
{
    public string Name { get; init; } = null!;
    public DateTimeOffset CreatedWhen { get; init; }
    public long DepartmentCount { get; init; }
    public AddressDto Address { get; init; } = null!;
}