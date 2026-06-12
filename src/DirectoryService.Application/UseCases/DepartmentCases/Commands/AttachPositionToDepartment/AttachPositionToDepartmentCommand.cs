using Shared.Core;

namespace DirectoryService.Application.UseCases.DepartmentCases.Commands.AttachPositionToDepartment;

public record AttachPositionToDepartmentCommand(Guid DepartmentId, Guid PositionId) : ICommand;