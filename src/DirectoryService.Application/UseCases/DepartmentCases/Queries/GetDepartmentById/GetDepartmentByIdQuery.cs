using Core.Abstractions;
using DirectoryService.Contracts.DepartmentContracts;

namespace DirectoryService.Application.UseCases.DepartmentCases.Queries.GetDepartmentById;

public record GetDepartmentByIdQuery(Guid Id) : IQuery<GetDepartmentDto>;