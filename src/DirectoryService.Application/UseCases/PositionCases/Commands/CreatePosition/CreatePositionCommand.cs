using DirectoryService.Contracts.PositionContracts;
using Shared.Core;

namespace DirectoryService.Application.UseCases.PositionCases.Commands.CreatePosition;

public record CreatePositionCommand(CreatePositionRequest Request) : ICommand;