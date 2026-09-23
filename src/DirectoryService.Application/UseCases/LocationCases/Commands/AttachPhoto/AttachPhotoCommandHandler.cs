using Core.Abstractions;
using Core.Database;
using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Contracts.LocationContracts;
using FileService.Contracts.HttpCommunication;
using Microsoft.Extensions.Logging;
using SharedKernel;

namespace DirectoryService.Application.UseCases.LocationCases.Commands.AttachPhoto;

public class AttachPhotoCommandHandler : ICommandHandler<AttachPhotoCommand, AttachPhotoResponse>
{
    private readonly ILocationRepository _locationRepository;
    private readonly ITransactionManager _transactionManager;
    private readonly ILogger<AttachPhotoCommandHandler> _logger;
    private readonly IFileCommunicationService _fileCommunicationService;

    public AttachPhotoCommandHandler(
        ILocationRepository locationRepository,
        ITransactionManager transactionManager,
        ILogger<AttachPhotoCommandHandler> logger,
        IFileCommunicationService fileCommunicationService)
    {
        _locationRepository = locationRepository;
        _transactionManager = transactionManager;
        _logger = logger;
        _fileCommunicationService = fileCommunicationService;
    }

    public async Task<Result<AttachPhotoResponse, Error>> HandleAsync(
        AttachPhotoCommand command, 
        CancellationToken cancellationToken)
    {
        if (command.Request.PhotoAssetId == Guid.Empty)
            return Error.Validation("photo.asset.id.invalid", "PhotoAssetId не может быть пустым");
        
        var locationResult = await _locationRepository
            .GetByAsync(l => l.Id == command.LocationId, cancellationToken);
        if (locationResult.IsFailure)
            return locationResult.Error;

        var existsResult = await _fileCommunicationService.CheckMediaAssetExists(
            command.Request.PhotoAssetId, 
            cancellationToken);

        if (existsResult.IsFailure)
            return existsResult.Error;

        if (!existsResult.Value.AssetExists)
            return Error.NotFound();

        var transaction = await _transactionManager.BeginTransactionAsync(cancellationToken);
        if (transaction.IsFailure)
            return transaction.Error;

        using var transactionScope = transaction.Value;

        locationResult.Value.AttachPhotoId(command.Request.PhotoAssetId);

        var saveResult = await _transactionManager.SaveChangesAsync(cancellationToken);
        if (saveResult.IsFailure)
            return saveResult.Error;

        var commitResult = transactionScope.Commit();
        if (commitResult.IsFailure)
            return commitResult.Error;

        _logger.LogInformation("Фото {PhotoAssetId} приклеплено к локации {LocationId}", 
            command.Request.PhotoAssetId, command.LocationId);
        
        return new AttachPhotoResponse(locationResult.Value.Id);
    }
}