using Core.Abstractions;
using DirectoryService.Contracts;

namespace DirectoryService.Application.UseCases.LocationCases.Commands.AttachPhoto;

public record AttachPhotoCommand(Guid LocationId, AttachPhotoRequest Request) : ICommand;