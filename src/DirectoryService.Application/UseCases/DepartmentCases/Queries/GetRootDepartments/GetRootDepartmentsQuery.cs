using DirectoryService.Contracts.DepartmentContracts;
using Shared.Core;

namespace DirectoryService.Application.UseCases.DepartmentCases.Queries.GetRootDepartments;

public record GetRootDepartmentsQuery : IQuery<IReadOnlyList<DepartmentTreeItemDto>>;