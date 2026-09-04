using Core.Abstractions;

namespace DirectoryService.Application.UseCases.PositionCases.Commands.RestorePosition;

public record RestorePositionCommand(Guid PositionId) : ICommand;