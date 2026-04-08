namespace DirectoryService.Contracts.LocationContracts;

public record CreateLocationRequest(
    string Name,
    AddressDto Address,
    string Timezone);