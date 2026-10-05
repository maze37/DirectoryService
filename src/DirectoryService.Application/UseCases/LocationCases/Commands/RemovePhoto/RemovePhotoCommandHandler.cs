using Core.Abstractions;
using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Abstractions.Database;
using DirectoryService.Contracts;
using DirectoryService.Contracts.LocationContracts;
using Microsoft.Extensions.Logging;
using SharedKernel;

namespace DirectoryService.Application.UseCases.LocationCases.Commands.RemovePhoto;

public class RemovePhotoCommandHandler : ICommandHandler<RemovePhotoCommand, RemovePhotoResponse>
{
    private readonly ILocationRepository _locationRepository;
    private readonly ITransactionManager _transactionManager;
    private readonly ILogger<RemovePhotoCommandHandler> _logger;

    public RemovePhotoCommandHandler(
        ILocationRepository locationRepository,
        ITransactionManager transactionManager,
        ILogger<RemovePhotoCommandHandler> logger)
    {
        _locationRepository = locationRepository;
        _transactionManager = transactionManager;
        _logger = logger;
    }

    public async Task<Result<RemovePhotoResponse, Error>> HandleAsync(
        RemovePhotoCommand command, 
        CancellationToken cancellationToken)
    {
        var locationResult = await _locationRepository
            .GetByAsync(l => l.Id == command.LocationId, cancellationToken);
        if (locationResult.IsFailure)
            return locationResult.Error;

        var transaction = await _transactionManager.BeginTransactionAsync(cancellationToken);
        if (transaction.IsFailure)
            return transaction.Error;

        locationResult.Value.RemovePhotoId();

        var commitResult = await _transactionManager.CommitTransactionAsync(cancellationToken);
        if (commitResult.IsFailure)
            return commitResult.Error;

        _logger.LogInformation("Фото удалено с локации {LocationId}", command.LocationId);
        
        return new RemovePhotoResponse(locationResult.Value.Id);
    }
}