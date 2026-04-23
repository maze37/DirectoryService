using DirectoryService.Contracts.DepartmentContracts;
using Shared.Core;

namespace DirectoryService.Application.UseCases.DepartmentCases.CreateDepartment;

public record CreateDepartmentCommand(CreateDepartmentRequest Request) : ICommand;