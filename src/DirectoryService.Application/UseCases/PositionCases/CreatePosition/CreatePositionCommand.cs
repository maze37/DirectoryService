using DirectoryService.Contracts.PositionContracts;
using Shared.Core;

namespace DirectoryService.Application.UseCases.PositionCases.CreatePosition;

public record CreatePositionCommand(CreatePositionRequest Request) : ICommand;