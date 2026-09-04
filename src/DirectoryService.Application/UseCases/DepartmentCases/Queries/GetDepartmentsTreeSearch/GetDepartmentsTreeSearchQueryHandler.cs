using Core.Abstractions;
using DirectoryService.Application.Abstractions.Database;
using DirectoryService.Contracts.DepartmentContracts;
using Microsoft.EntityFrameworkCore;

namespace DirectoryService.Application.UseCases.DepartmentCases.Queries.GetDepartmentsTreeSearch;

public class GetDepartmentsTreeSearchQueryHandler : IQueryHandler<GetDepartmentsTreeSearchQuery, IReadOnlyList<DepartmentTreeItemDto>>
{
    private readonly IReadDbContext _readDbContext;
    
    public GetDepartmentsTreeSearchQueryHandler(IReadDbContext readDbContext)
    {
        _readDbContext = readDbContext;
    }

    public async Task<IReadOnlyList<DepartmentTreeItemDto>> HandleAsync(
        GetDepartmentsTreeSearchQuery query,
        CancellationToken cancellationToken = default)
    {
        return await _readDbContext.DepartmentsRead
            .Where(d => EF.Functions.ILike(d.DepartmentName, $"%{query.Q}%"))
            .Select(d => new DepartmentTreeItemDto(
                d.Id,
                d.DepartmentName,
                d.Slug,
                d.Path,
                d.Depth,
                d.ChildrenCount > 0,
                d.ChildrenCount))
            .ToListAsync(cancellationToken);
    }
}