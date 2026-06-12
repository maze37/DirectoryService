using Shared.Core;

namespace DirectoryService.Application.UseCases.DepartmentCases.Commands.DeleteDepartment;

public record DeleteDepartmentCommand(Guid Id) : ICommand;