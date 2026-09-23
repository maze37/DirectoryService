using Core.Abstractions;
using Core.Database;
using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Contracts;
using DirectoryService.Contracts.LocationContracts;
using FileService.Contracts.HttpCommunication;
using Microsoft.Extensions.Logging;
using SharedKernel;

namespace DirectoryService.Application.UseCases.LocationCases.Commands.UpdatePhoto;

public class UpdatePhotoCommandHandler : ICommandHandler<UpdatePhotoCommand, UpdatePhotoResponse>
{
    private readonly ILocationRepository _locationRepository;
    private readonly ITransactionManager _transactionManager;
    private readonly ILogger<UpdatePhotoCommandHandler> _logger;
    private readonly IFileCommunicationService _fileCommunicationService;

    public UpdatePhotoCommandHandler(
        ILocationRepository locationRepository,
        ITransactionManager transactionManager,
        ILogger<UpdatePhotoCommandHandler> logger,
        IFileCommunicationService fileCommunicationService)
    {
        _locationRepository = locationRepository;
        _transactionManager = transactionManager;
        _logger = logger;
        _fileCommunicationService = fileCommunicationService;
    }

    public async Task<Result<UpdatePhotoResponse, Error>> HandleAsync(
        UpdatePhotoCommand command, 
        CancellationToken cancellationToken)
    {
        if (command.Request.NewPhotoAssetId == Guid.Empty)
            return Error.Validation("photo.asset.id.invalid", "NewPhotoAssetId не может быть пустым");
        
        var locationResult = await _locationRepository
            .GetByAsync(l => l.Id == command.LocationId, cancellationToken);
        if (locationResult.IsFailure)
            return locationResult.Error;

        var existsResult = await _fileCommunicationService.CheckMediaAssetExistsAndReady(
            command.Request.NewPhotoAssetId, 
            cancellationToken);

        if (existsResult.IsFailure)
            return existsResult.Error;

        if (!existsResult.Value.AssetExists)
            return Error.NotFound();
        
        if (!existsResult.Value.IsReady)
            return Error.Validation("photo.asset.not_ready", "Файл ещё не готов к использованию");

        var transaction = await _transactionManager.BeginTransactionAsync(cancellationToken);
        if (transaction.IsFailure)
            return transaction.Error;

        using var transactionScope = transaction.Value;

        locationResult.Value.UpdatePhotoId(command.Request.NewPhotoAssetId);

        var saveResult = await _transactionManager.SaveChangesAsync(cancellationToken);
        if (saveResult.IsFailure)
            return saveResult.Error;

        var commitResult = transactionScope.Commit();
        if (commitResult.IsFailure)
            return commitResult.Error;

        _logger.LogInformation("К локации {LocationId} приклеплено новое фото {PhotoAssetId}",
            command.LocationId, command.Request.NewPhotoAssetId);
        
        return new UpdatePhotoResponse(locationResult.Value.Id);
    }
}