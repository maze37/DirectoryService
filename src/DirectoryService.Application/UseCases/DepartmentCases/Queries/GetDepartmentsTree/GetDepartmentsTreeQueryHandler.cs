using DirectoryService.Application.Abstractions.Database;
using DirectoryService.Contracts.DepartmentContracts;
using Microsoft.EntityFrameworkCore;
using Shared.Core;

namespace DirectoryService.Application.UseCases.DepartmentCases.Queries.GetDepartmentsTree;

public class GetDepartmentsTreeQueryHandler : IQueryHandler<GetDepartmentsTreeQuery, IReadOnlyList<DepartmentTreeItemDto>>
{
    private readonly IReadDbContext _readDbContext;

    public GetDepartmentsTreeQueryHandler(IReadDbContext readDbContext)
    {
        _readDbContext = readDbContext;
    }

    public async Task<IReadOnlyList<DepartmentTreeItemDto>> HandleAsync(
        GetDepartmentsTreeQuery query,
        CancellationToken cancellationToken)
    {
        return await _readDbContext.DepartmentsRead
            .Where(d => d.ParentId == null)
            .Select(d => new DepartmentTreeItemDto(
                d.Id,
                d.DepartmentName.Value,
                d.Slug.Value,
                d.Path.Value,
                d.Depth,
                d.ChildrenCount > 0,
                d.ChildrenCount))
            .ToListAsync(cancellationToken);
    }
}