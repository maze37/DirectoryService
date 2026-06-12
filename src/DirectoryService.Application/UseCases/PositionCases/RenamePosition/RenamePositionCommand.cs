using DirectoryService.Contracts.PositionContracts;
using Shared.Core;

namespace DirectoryService.Application.UseCases.PositionCases.RenamePosition;

public record RenamePositionCommand(Guid Id, RenamePositionRequest Request) : ICommand;