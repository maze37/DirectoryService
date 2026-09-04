using Core.Abstractions;
using DirectoryService.Application.Abstractions.Database;
using DirectoryService.Contracts.Constants;
using DirectoryService.Contracts.DepartmentContracts;
using Microsoft.EntityFrameworkCore;
using Shared.Exceptions;

namespace DirectoryService.Application.UseCases.DepartmentCases.Queries.GetDepartments;

public class GetDepartmentsQueryHandler : IQueryHandler<GetDepartmentsQuery, PagedResult<DepartmentListItemDto>>
{
    private readonly IReadDbContext _readDbContext;

    public GetDepartmentsQueryHandler(IReadDbContext readDbContext)
    {
        _readDbContext = readDbContext;
    }
    
    public async Task<PagedResult<DepartmentListItemDto>?> HandleAsync(
        GetDepartmentsQuery query,
        CancellationToken cancellationToken)
    {
        var departmentsQuery = _readDbContext.DepartmentsRead;

        if (query.Request.Search?.Length >= LenghtConstants.MAXLENGHT)
        {
            throw new ValidationException($"Длина поискового запроса не должна превышать {LenghtConstants.MAXLENGHT} символов");
        }

        if (!string.IsNullOrWhiteSpace(query.Request.Search))
            departmentsQuery = departmentsQuery.Where(d => EF.Functions
                .ILike(d.DepartmentName, $"%{query.Request.Search.ToLower()}%"));

        var sortBy = query.Request.SortBy?.ToLower();
        var sortDir = query.Request.SortDir?.ToLower();
        
        // Валидация sortDir, если он передан
        if (!string.IsNullOrEmpty(sortDir) && sortDir != "asc" && sortDir != "desc")
        {
            throw new ValidationException($"Недопустимое направление сортировки: sortDir={sortDir}. Допустимые значения: asc, desc");
        }

        // Применяем сортировку
        departmentsQuery = sortBy switch
        {
            "createdwhen" => sortDir == "desc" 
                ? departmentsQuery.OrderByDescending(d => d.CreatedWhen)
                : departmentsQuery.OrderBy(d => d.CreatedWhen),
    
            "name" => sortDir == "desc" 
                ? departmentsQuery.OrderByDescending(d => d.DepartmentName)
                : departmentsQuery.OrderBy(d => d.DepartmentName),
    
            null => departmentsQuery.OrderBy(d => d.DepartmentName), // дефолт, игнорируем sortDir
    
            _ => throw new ValidationException($"Недопустимое поле сортировки: sortBy={sortBy}. Допустимые значения: name, createdwhen")
        };
        
        var totalCount = await departmentsQuery.LongCountAsync(cancellationToken);
        
        if (query.Request.Pagination.Page < 1)
        {
            throw new ValidationException("Номер страницы должен быть больше 0");
        }
        
        if (query.Request.Pagination.PageSize <= 0)
        {
            throw new ValidationException("Размер страницы должен быть больше 0");
        }

        if (query.Request.Pagination.PageSize > 100)
        {
            throw new ValidationException("Размер страницы не должен превышать 100");
        }
        
        var items = await departmentsQuery
            .Skip((query.Request.Pagination.Page - 1) * query.Request.Pagination.PageSize)
            .Take(query.Request.Pagination.PageSize)
            .Select(dto => new DepartmentListItemDto(
                Name: dto.DepartmentName.Value,
                Slug: dto.Slug.Value,
                Path: dto.Path,
                CreatedWhen: dto.CreatedWhen
            ))
            .ToListAsync(cancellationToken);

        return new PagedResult<DepartmentListItemDto>(
            items,
            totalCount,
            query.Request.Pagination);
    }
}