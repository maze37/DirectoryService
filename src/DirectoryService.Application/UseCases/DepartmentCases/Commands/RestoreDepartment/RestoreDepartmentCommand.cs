using Shared.Core;

namespace DirectoryService.Application.UseCases.DepartmentCases.Commands.RestoreDepartment;

public record RestoreDepartmentCommand(Guid DepartmentId) : ICommand;