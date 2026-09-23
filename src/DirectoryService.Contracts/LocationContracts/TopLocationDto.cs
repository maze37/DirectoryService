namespace DirectoryService.Contracts.LocationContracts;

public record TopLocationDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = null!;
    public AddressDto Address { get; init; } = null!;
    public int DepartmentCount { get; init; }
    public Guid? PhotoAssetId { get; init; }
    public MediaAssetDto MediaAssetDto { get; init; } = null!;
}