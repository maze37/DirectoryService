using DirectoryService.Contracts.DepartmentContracts;
using Shared.Core;

namespace DirectoryService.Application.UseCases.DepartmentCases.Queries.GetDepartmentsAncestors;

public record GetDepartmentsAncestorsQuery(Guid DepartmentId) : IQuery<IReadOnlyList<DepartmentTreeItemDto>?>;