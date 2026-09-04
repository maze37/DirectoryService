using Core.Abstractions;
using DirectoryService.Contracts.DepartmentContracts;

namespace DirectoryService.Application.UseCases.DepartmentCases.Commands.CreateDepartment;

public record CreateDepartmentCommand(CreateDepartmentRequest Request) : ICommand;