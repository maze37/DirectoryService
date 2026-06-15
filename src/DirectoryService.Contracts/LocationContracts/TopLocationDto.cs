namespace DirectoryService.Contracts.LocationContracts;

public record TopLocationDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = null!;
    public AddressDto Address { get; init; } = null!;
    public int DepartmentCount { get; init; }
}