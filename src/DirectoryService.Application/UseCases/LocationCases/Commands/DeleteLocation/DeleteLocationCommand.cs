using Core.Abstractions;

namespace DirectoryService.Application.UseCases.LocationCases.Commands.DeleteLocation;

public record DeleteLocationCommand(Guid Id) : ICommand;