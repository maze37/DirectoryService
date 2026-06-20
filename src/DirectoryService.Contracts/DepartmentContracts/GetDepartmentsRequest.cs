using DirectoryService.Contracts.Constants;

namespace DirectoryService.Contracts.DepartmentContracts;

public record GetDepartmentsRequest(
    string? Search,
    string? SortBy,
    string? SortDir,
    PaginationRequest Pagination);