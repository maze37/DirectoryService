using DirectoryService.Contracts.Constants;
using DirectoryService.Contracts.DepartmentContracts;
using Shared.Core;

namespace DirectoryService.Application.UseCases.DepartmentCases.Queries.GetDepartmentsTreeSearch;

public record GetDepartmentsTreeSearchQuery(string Q) : IQuery<IReadOnlyList<DepartmentTreeItemDto>>;