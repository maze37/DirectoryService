using Core.Abstractions;
using DirectoryService.Contracts;

namespace DirectoryService.Application.UseCases.LocationCases.Commands.UpdatePhoto;

public record UpdatePhotoCommand(Guid LocationId, UpdatePhotoRequest Request) : ICommand;