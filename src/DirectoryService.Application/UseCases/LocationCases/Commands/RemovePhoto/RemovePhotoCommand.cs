using Core.Abstractions;

namespace DirectoryService.Application.UseCases.LocationCases.Commands.RemovePhoto;

public record RemovePhotoCommand(Guid LocationId) : ICommand;