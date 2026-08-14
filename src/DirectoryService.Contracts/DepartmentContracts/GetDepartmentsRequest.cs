using DirectoryService.Contracts.Constants;

namespace DirectoryService.Contracts.DepartmentContracts;

public class GetDepartmentsRequest
{
    public string? Search { get; init; }
    public string? SortBy { get; init; }
    public string? SortDir { get; init; }
    public PaginationRequest Pagination { get; init; } = new(1, 10);
}