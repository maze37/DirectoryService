using Shared.Core;

namespace DirectoryService.Application.UseCases.PositionCases.Commands.DeletePosition;

public record DeletePositionCommand(Guid Id) : ICommand;