using Core.Abstractions;
using DirectoryService.Contracts.Constants;
using DirectoryService.Contracts.DepartmentContracts;

namespace DirectoryService.Application.UseCases.DepartmentCases.Queries.GetDepartments;

public record GetDepartmentsQuery(GetDepartmentsRequest Request) : IQuery<PagedResult<DepartmentListItemDto>>;