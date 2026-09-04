using Core.Abstractions;
using DirectoryService.Contracts.DepartmentContracts;

namespace DirectoryService.Application.UseCases.DepartmentCases.Queries.GetDepartmentsAncestors;

public record GetDepartmentsAncestorsQuery(Guid DepartmentId) : IQuery<IReadOnlyList<DepartmentTreeItemDto>?>;