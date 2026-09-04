using Core.Abstractions;
using DirectoryService.Contracts.PositionContracts;

namespace DirectoryService.Application.UseCases.PositionCases.Commands.RenamePosition;

public record RenamePositionCommand(Guid Id, RenamePositionRequest Request) : ICommand;