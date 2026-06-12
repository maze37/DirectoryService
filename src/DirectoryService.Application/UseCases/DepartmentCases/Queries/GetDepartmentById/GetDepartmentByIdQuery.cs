using DirectoryService.Contracts.DepartmentContracts;
using Shared.Core;

namespace DirectoryService.Application.UseCases.DepartmentCases.Queries.GetDepartmentById;

public record GetDepartmentByIdQuery(Guid Id) : IQuery<GetDepartmentDto>;