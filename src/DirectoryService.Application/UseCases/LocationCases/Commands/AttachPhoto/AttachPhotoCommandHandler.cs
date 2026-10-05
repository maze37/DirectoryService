using Core.Abstractions;
using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Abstractions.Database;
using DirectoryService.Contracts.LocationContracts;
using DirectoryService.Application.ReadModels;
using Microsoft.Extensions.Logging;
using SharedKernel;

namespace DirectoryService.Application.UseCases.LocationCases.Commands.AttachPhoto;

public class AttachPhotoCommandHandler : ICommandHandler<AttachPhotoCommand, AttachPhotoResponse>
{
    private readonly ILocationRepository _locationRepository;
    private readonly ITransactionManager _transactionManager;
    private readonly ILogger<AttachPhotoCommandHandler> _logger;
    private readonly IAssetStateRepository _assetStateRepository;

    public AttachPhotoCommandHandler(
        ILocationRepository locationRepository,
        ITransactionManager transactionManager,
        ILogger<AttachPhotoCommandHandler> logger,
        IAssetStateRepository assetStateRepository)
    {
        _locationRepository = locationRepository;
        _transactionManager = transactionManager;
        _logger = logger;
        _assetStateRepository = assetStateRepository;
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

        var asset = await _assetStateRepository.GetByIdAsync(
            command.Request.PhotoAssetId, cancellationToken);

        if (asset is null)
            return Error.Conflict("photo.asset.state_unknown",
                "Готовность файла ещё не подтверждена. Повторите попытку позже");

        if (asset.Status == AssetStatus.Deleted)
            return Error.Conflict("photo.asset.deleted", "Файл удалён");

        if (asset.Status != AssetStatus.Ready)
            return Error.Conflict("photo.asset.not_ready", "Файл ещё не готов к использованию");

        if (asset.EntityId != command.LocationId ||
            !string.Equals(asset.EntityType, "location", StringComparison.OrdinalIgnoreCase))
            return Error.Validation("photo.asset.owner_mismatch",
                "Файл не принадлежит этой локации");

        var transaction = await _transactionManager.BeginTransactionAsync(cancellationToken);
        if (transaction.IsFailure)
            return transaction.Error;

        locationResult.Value.AttachPhotoId(command.Request.PhotoAssetId);

        var commitResult = await _transactionManager.CommitTransactionAsync(cancellationToken);
        if (commitResult.IsFailure)
            return commitResult.Error;

        _logger.LogInformation("Фото {PhotoAssetId} приклеплено к локации {LocationId}", 
            command.Request.PhotoAssetId, command.LocationId);
        
        return new AttachPhotoResponse(locationResult.Value.Id);
    }
}