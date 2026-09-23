using Core.Abstractions;
using DirectoryService.Contracts;
using DirectoryService.Contracts.LocationContracts;

namespace DirectoryService.Application.UseCases.LocationCases.Commands.AttachPhoto;

public record AttachPhotoCommand(Guid LocationId, AttachPhotoRequest Request) : ICommand;