using DirectoryService.Contracts.DepartmentContracts;
using Shared.Core;

namespace DirectoryService.Application.UseCases.DepartmentCases.Queries.GetDepartmentsTree;

public record GetDepartmentsTreeQuery : IQuery<IReadOnlyList<DepartmentTreeItemDto>>;