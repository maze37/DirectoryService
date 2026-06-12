using DirectoryService.Contracts.DepartmentContracts;
using Shared.Core;

namespace DirectoryService.Application.UseCases.DepartmentCases.Commands.CreateDepartment;

public record CreateDepartmentCommand(CreateDepartmentRequest Request) : ICommand;