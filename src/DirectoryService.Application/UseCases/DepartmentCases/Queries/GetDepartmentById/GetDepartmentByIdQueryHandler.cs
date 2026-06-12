using DirectoryService.Application.Abstractions.Database;
using DirectoryService.Contracts.DepartmentContracts;
using Microsoft.EntityFrameworkCore;
using Shared.Core;

namespace DirectoryService.Application.UseCases.DepartmentCases.Queries.GetDepartmentById;

public class GetDepartmentByIdQueryHandler : IQueryHandler<GetDepartmentByIdQuery, GetDepartmentDto>
{
    private readonly IReadDbContext _readDbContext;

    public GetDepartmentByIdQueryHandler(IReadDbContext readDbContext)
    {
        _readDbContext = readDbContext;
    }

    public async Task<GetDepartmentDto?> HandleAsync(
        GetDepartmentByIdQuery query,
        CancellationToken cancellationToken = default)
    {
        var department = await _readDbContext.DepartmentsRead
            .Where(d => d.Id == query.Id)
            .Select(d => new GetDepartmentDto
            {
                Id = d.Id,
                DepartmentName = d.DepartmentName,
                Slug = d.Slug,
                ParentId = d.ParentId,
                Path = d.Path,
                Depth = d.Depth,
                ChildrenCount = d.ChildrenCount,
                IsActive = d.IsActive,
                CreatedWhen = d.CreatedWhen,
                UpdatedWhen = d.UpdatedWhen
            })
            .FirstOrDefaultAsync(cancellationToken);

        return department; // null если не найден
    }
}