using Core.Abstractions;
using DirectoryService.Contracts;
using DirectoryService.Contracts.LocationContracts;

namespace DirectoryService.Application.UseCases.LocationCases.Commands.UpdatePhoto;

public record UpdatePhotoCommand(Guid LocationId, UpdatePhotoRequest Request) : ICommand;