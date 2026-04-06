namespace DirectoryService.Contracts.Location;

public record CreateLocationRequest(
    string Name,
    AddressDto Address,
    string Timezone);