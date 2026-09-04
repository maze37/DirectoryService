using Core.Abstractions;
using DirectoryService.Application.Abstractions.Database;
using DirectoryService.Contracts.DepartmentContracts;
using Microsoft.EntityFrameworkCore;

namespace DirectoryService.Application.UseCases.DepartmentCases.Queries.GetDepartmentsChildren;

public class GetDepartmentsChildrenQueryHandler : IQueryHandler<GetDepartmentsChildrenQuery, IReadOnlyList<DepartmentTreeItemDto>?>
{
    private readonly IReadDbContext _readDbContext;

    public GetDepartmentsChildrenQueryHandler(IReadDbContext readDbContext)
    {
        _readDbContext = readDbContext;
    }

    public async Task<IReadOnlyList<DepartmentTreeItemDto>?> HandleAsync(
        GetDepartmentsChildrenQuery query, 
        CancellationToken cancellationToken)
    {
        bool exists = await _readDbContext.DepartmentsRead
            .AnyAsync(x => x.Id == query.ParentDepartmentId, cancellationToken);
        if (!exists) 
            return null;
        
        return await _readDbContext.DepartmentsRead
            .Where(x => x.ParentId == query.ParentDepartmentId)
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