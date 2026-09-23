using Core.Abstractions;
using Core.Database;
using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Contracts;
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
        var locationResult = await _locationRepository
            .GetByAsync(l => l.Id == command.LocationId, cancellationToken);
        if (locationResult.IsFailure)
            return locationResult.Error;

        if (command.Request.PhotoAssetId != Guid.Empty)
        {
            var existsResult = await _fileCommunicationService.CheckMediaAssetExists(
                    command.Request.PhotoAssetId, 
                    cancellationToken);

            if (existsResult.IsFailure)
                return existsResult.Error;

            if (!existsResult.Value.AssetExists)
                return Error.NotFound();
        }

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