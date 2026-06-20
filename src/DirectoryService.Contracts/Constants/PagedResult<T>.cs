namespace DirectoryService.Contracts.Constants;

public record PagedResult<T>(
    List<T> Items,
    long TotalCount,
    PaginationRequest Pagination);