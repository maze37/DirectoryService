using Shared.Core;

namespace DirectoryService.Application.UseCases.DepartmentCases.DeleteDepartment;

public record DeleteDepartmentCommand(Guid Id) : ICommand;