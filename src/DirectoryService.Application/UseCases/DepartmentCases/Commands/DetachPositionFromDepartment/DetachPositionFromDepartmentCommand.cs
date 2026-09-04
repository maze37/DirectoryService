using Core.Abstractions;

namespace DirectoryService.Application.UseCases.DepartmentCases.Commands.DetachPositionFromDepartment;

public record DetachPositionFromDepartmentCommand(Guid DepartmentId, Guid PositionId) : ICommand;