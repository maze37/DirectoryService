using Core.Abstractions;
using DirectoryService.Contracts.DepartmentContracts;

namespace DirectoryService.Application.UseCases.DepartmentCases.Queries.GetRootDepartments;

public record GetRootDepartmentsQuery : IQuery<IReadOnlyList<DepartmentTreeItemDto>>;