namespace DirectoryService.Contracts.LocationContracts;

public record GetLocationDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = null!;
    public string Country { get; init; } = null!;
    public string City { get; init; } = null!;
    public string Street { get; init; } = null!;
    public string Building { get; init; } = null!;
    public string? Office { get; init; }
    public string? PostalCode { get; init; }
    public string Timezone { get; init; } = null!;
    public bool IsActive { get; init; }
    public DateTimeOffset CreatedWhen { get; init; }
    public DateTimeOffset UpdatedWhen { get; init; }
}