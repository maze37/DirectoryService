namespace DirectoryService.Contracts.LocationContracts;

public record AddressDto
{
    public string Country { get; init; } = null!;
    public string City { get; init; } = null!;
    public string Street { get; init; } = null!;
    public string Building { get; init; } = null!;
    public string? Office { get; init; }
    public string? PostalCode { get; init; }
}