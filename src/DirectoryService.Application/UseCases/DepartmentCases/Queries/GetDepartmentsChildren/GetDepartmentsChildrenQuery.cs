using DirectoryService.Contracts.DepartmentContracts;
using Shared.Core;

namespace DirectoryService.Application.UseCases.DepartmentCases.Queries.GetDepartmentsChildren;

public record GetDepartmentsChildrenQuery(Guid ParentDepartmentId) : IQuery<IReadOnlyList<DepartmentTreeItemDto>?>;