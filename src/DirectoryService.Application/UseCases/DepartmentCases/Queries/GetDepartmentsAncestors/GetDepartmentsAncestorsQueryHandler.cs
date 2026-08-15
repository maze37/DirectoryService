using DirectoryService.Application.Abstractions.Database;
using DirectoryService.Contracts.DepartmentContracts;
using Microsoft.EntityFrameworkCore;
using Shared.Core;

namespace DirectoryService.Application.UseCases.DepartmentCases.Queries.GetDepartmentsAncestors;

public class GetDepartmentsAncestorsQueryHandler : IQueryHandler<GetDepartmentsAncestorsQuery, IReadOnlyList<DepartmentTreeItemDto>?>
{
    private readonly IReadDbContext _readDbContext;
    
    public GetDepartmentsAncestorsQueryHandler(IReadDbContext readDbContext)
    {
        _readDbContext = readDbContext;
    }

    public async Task<IReadOnlyList<DepartmentTreeItemDto>?> HandleAsync(
        GetDepartmentsAncestorsQuery query,
        CancellationToken cancellationToken)
    {
        var currentDepartmentPath = await _readDbContext.DepartmentsRead
            .Where(x => x.Id == query.DepartmentId)
            .Select(x => x.Path.Value)
            .FirstOrDefaultAsync(cancellationToken);
        
        if (currentDepartmentPath is null)
            return null;
        
        var allSlugsInPath = currentDepartmentPath
            .Split('.', StringSplitOptions.RemoveEmptyEntries);
        
        var ancestorSlugs = allSlugsInPath.SkipLast(1).ToList();
        
        if (!ancestorSlugs.Any())
            return [];
        
        return await _readDbContext.DepartmentsRead
            .Where(d => ancestorSlugs.Contains(d.Slug))
            .OrderBy(d => d.Depth)
            .Select(d => new DepartmentTreeItemDto(
                d.Id,
                d.DepartmentName.Value,
                d.Slug,
                d.Path,
                d.Depth,
                d.ChildrenCount > 0,
                d.ChildrenCount))
            .ToListAsync(cancellationToken);
    }
}