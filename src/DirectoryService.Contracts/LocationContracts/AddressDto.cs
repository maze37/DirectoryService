namespace DirectoryService.Contracts.LocationContracts;

public record AddressDto(
    string Country,
    string City,
    string Street,
    string Building,
    string? Office,
    string? PostalCode);