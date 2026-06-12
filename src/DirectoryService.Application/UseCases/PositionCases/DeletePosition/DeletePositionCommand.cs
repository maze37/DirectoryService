using Shared.Core;

namespace DirectoryService.Application.UseCases.PositionCases.DeletePosition;

public record DeletePositionCommand(Guid Id) : ICommand;