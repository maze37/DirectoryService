using Core.Abstractions;
using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Abstractions.Database;
using DirectoryService.Contracts.LocationContracts;
using DirectoryService.Application.ReadModels;
using Microsoft.Extensions.Logging;
using SharedKernel;

namespace DirectoryService.Application.UseCases.LocationCases.Commands.UpdatePhoto;

public class UpdatePhotoCommandHandler : ICommandHandler<UpdatePhotoCommand, UpdatePhotoResponse>
{
    private readonly ILocationRepository _locationRepository;
    private readonly ITransactionManager _transactionManager;
    private readonly ILogger<UpdatePhotoCommandHandler> _logger;
    private readonly IAssetStateRepository _assetStateRepository;

    public UpdatePhotoCommandHandler(
        ILocationRepository locationRepository,
        ITransactionManager transactionManager,
        ILogger<UpdatePhotoCommandHandler> logger,
        IAssetStateRepository assetStateRepository)
    {
        _locationRepository = locationRepository;
        _transactionManager = transactionManager;
        _logger = logger;
        _assetStateRepository = assetStateRepository;
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

        var asset = await _assetStateRepository.GetByIdAsync(
            command.Request.NewPhotoAssetId, cancellationToken);

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

        locationResult.Value.UpdatePhotoId(command.Request.NewPhotoAssetId);

        var commitResult = await _transactionManager.CommitTransactionAsync(cancellationToken);
        if (commitResult.IsFailure)
            return commitResult.Error;

        _logger.LogInformation("К локации {LocationId} приклеплено новое фото {PhotoAssetId}",
            command.LocationId, command.Request.NewPhotoAssetId);
        
        return new UpdatePhotoResponse(locationResult.Value.Id);
    }
}