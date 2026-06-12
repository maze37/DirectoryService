using Shared.Core;

namespace DirectoryService.Application.UseCases.DepartmentCases.DetachPositionFromDepartment;

public record DetachPositionFromDepartmentCommand(Guid DepartmentId, Guid PositionId) : ICommand;