using DirectoryService.Application.Abstractions.Database;
using DirectoryService.Contracts.Constants;
using DirectoryService.Contracts.DepartmentContracts;
using Microsoft.EntityFrameworkCore;
using Shared.Core;
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

        departmentsQuery = (sortBy, sortDir) switch
        {
            ("createdwhen", "desc") => departmentsQuery.OrderByDescending(d => d.CreatedWhen),
            ("createdwhen", _)      => departmentsQuery.OrderBy(d => d.CreatedWhen),
    
            ("name", "desc") => departmentsQuery.OrderByDescending(d => d.DepartmentName),
            ("name", _)       => departmentsQuery.OrderBy(d => d.DepartmentName),
    
            (null, null) => departmentsQuery.OrderBy(d => d.DepartmentName), // дефолт по заданию
    
            _ => throw new ArgumentException($"Недопустимые параметры сортировки: sortBy={sortBy}, sortDir={sortDir}")
        };
        
        var totalCount = await departmentsQuery.LongCountAsync(cancellationToken);

        // Валидация пагинации
        if (query.Request.Pagination.Page < 1)
        {
            throw new ValidationException("Номер страницы должен быть больше 0");
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