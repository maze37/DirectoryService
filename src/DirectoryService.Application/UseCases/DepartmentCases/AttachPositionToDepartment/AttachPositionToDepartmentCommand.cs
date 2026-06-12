using Shared.Core;

namespace DirectoryService.Application.UseCases.DepartmentCases.AttachPositionToDepartment;

public record AttachPositionToDepartmentCommand(Guid DepartmentId, Guid PositionId) : ICommand;