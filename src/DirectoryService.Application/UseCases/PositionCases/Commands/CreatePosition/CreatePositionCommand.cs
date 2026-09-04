using Core.Abstractions;
using DirectoryService.Contracts.PositionContracts;

namespace DirectoryService.Application.UseCases.PositionCases.Commands.CreatePosition;

public record CreatePositionCommand(CreatePositionRequest Request) : ICommand;