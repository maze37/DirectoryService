using Core.Abstractions;

namespace DirectoryService.Application.UseCases.LocationCases.Commands.RestoreLocation;

public record RestoreLocationCommand(Guid LocationId) : ICommand;