using Shared.Core;

namespace DirectoryService.Application.UseCases.LocationCases.DeleteLocation;

public record DeleteLocationCommand(Guid Id) : ICommand;