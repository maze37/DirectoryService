using DirectoryService.Application.Abstractions.Database;
using DirectoryService.Contracts.DepartmentContracts;
using Microsoft.EntityFrameworkCore;
using Shared.Core;

namespace DirectoryService.Application.UseCases.DepartmentCases.Queries.GetRootDepartments;

public class GetRootDepartmentsQueryHandler : IQueryHandler<GetRootDepartmentsQuery, IReadOnlyList<DepartmentTreeItemDto>>
{
    private readonly IReadDbContext _readDbContext;

    public GetRootDepartmentsQueryHandler(IReadDbContext readDbContext)
    {
        _readDbContext = readDbContext;
    }

    public async Task<IReadOnlyList<DepartmentTreeItemDto>> HandleAsync(
        GetRootDepartmentsQuery query,
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