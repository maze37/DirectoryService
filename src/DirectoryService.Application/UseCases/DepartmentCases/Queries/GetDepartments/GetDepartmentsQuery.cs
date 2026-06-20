using DirectoryService.Contracts.Constants;
using DirectoryService.Contracts.DepartmentContracts;
using Shared.Core;

namespace DirectoryService.Application.UseCases.DepartmentCases.Queries.GetDepartments;

public record GetDepartmentsQuery(GetDepartmentsRequest Request) : IQuery<PagedResult<DepartmentListItemDto>>;