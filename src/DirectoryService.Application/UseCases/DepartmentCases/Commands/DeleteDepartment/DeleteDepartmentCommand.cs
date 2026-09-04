using Core.Abstractions;

namespace DirectoryService.Application.UseCases.DepartmentCases.Commands.DeleteDepartment;

public record DeleteDepartmentCommand(Guid Id) : ICommand;