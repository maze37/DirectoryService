using DirectoryService.Contracts.Constants;

namespace DirectoryService.Contracts.LocationContracts;

public record GetLocationsRequest(
    string? Search,
    int? MinDepartmentCount,
    string? SortBy,
    string? SortDir,
    PaginationRequest Pagination);