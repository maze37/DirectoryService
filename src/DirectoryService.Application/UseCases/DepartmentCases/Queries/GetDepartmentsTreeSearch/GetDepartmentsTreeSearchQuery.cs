using Core.Abstractions;
using DirectoryService.Contracts.DepartmentContracts;

namespace DirectoryService.Application.UseCases.DepartmentCases.Queries.GetDepartmentsTreeSearch;

public record GetDepartmentsTreeSearchQuery(string Q) : IQuery<IReadOnlyList<DepartmentTreeItemDto>>;