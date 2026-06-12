using Shared.Core;

namespace DirectoryService.Application.UseCases.DepartmentCases.Commands.DetachPositionFromDepartment;

public record DetachPositionFromDepartmentCommand(Guid DepartmentId, Guid PositionId) : ICommand;