using Core.Abstractions;
using DirectoryService.Contracts.DepartmentContracts;

namespace DirectoryService.Application.UseCases.DepartmentCases.Queries.GetDepartmentsChildren;

public record GetDepartmentsChildrenQuery(Guid ParentDepartmentId) : IQuery<IReadOnlyList<DepartmentTreeItemDto>?>;