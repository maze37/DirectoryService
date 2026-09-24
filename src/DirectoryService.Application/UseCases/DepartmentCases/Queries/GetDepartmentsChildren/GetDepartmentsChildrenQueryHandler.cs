using Core.Abstractions;
using DirectoryService.Application.Abstractions.Database;
using DirectoryService.Contracts.DepartmentContracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Application.UseCases.DepartmentCases.Queries.GetDepartmentsChildren;

public class GetDepartmentsChildrenQueryHandler : IQueryHandler<GetDepartmentsChildrenQuery, IReadOnlyList<DepartmentTreeItemDto>?>
{
    private readonly IReadDbContext _readDbContext;
    private readonly HybridCache _cache;
    private readonly ILogger<GetDepartmentsChildrenQueryHandler> _logger;

    public GetDepartmentsChildrenQueryHandler(
        IReadDbContext readDbContext, 
        HybridCache cache,
        ILogger<GetDepartmentsChildrenQueryHandler> logger)
    {
        _readDbContext = readDbContext;
        _cache = cache;
        _logger = logger;
    }

    public async Task<IReadOnlyList<DepartmentTreeItemDto>?> HandleAsync(
        GetDepartmentsChildrenQuery query, 
        CancellationToken cancellationToken)
    {
        return await _cache.GetOrCreateAsync<IReadOnlyList<DepartmentTreeItemDto>?>(
            $"{query.ParentDepartmentId}",
            async token =>
            {
                _logger.LogInformation("MISS CACHE: Не удалось получить данные из кэша");
                
                bool exists = await _readDbContext.DepartmentsRead
                    .AnyAsync(x => x.Id == query.ParentDepartmentId, token);
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
                    .ToListAsync(token);
            },
            new HybridCacheEntryOptions
            {
                Expiration = TimeSpan.FromMinutes(10),
                LocalCacheExpiration = TimeSpan.FromMinutes(2)
            },
            tags: ["departments-tree"],
            cancellationToken: cancellationToken);
    }
}