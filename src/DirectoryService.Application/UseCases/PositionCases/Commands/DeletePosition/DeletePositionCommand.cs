using Core.Abstractions;

namespace DirectoryService.Application.UseCases.PositionCases.Commands.DeletePosition;

public record DeletePositionCommand(Guid Id) : ICommand;