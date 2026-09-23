using Core.Abstractions;
using Core.Database;
using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Contracts;
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

        using var transactionScope = transaction.Value;

        locationResult.Value.RemovePhotoId();

        var saveResult = await _transactionManager.SaveChangesAsync(cancellationToken);
        if (saveResult.IsFailure)
            return saveResult.Error;

        var commitResult = transactionScope.Commit();
        if (commitResult.IsFailure)
            return commitResult.Error;

        _logger.LogInformation("Фото удалено с локации {LocationId}", command.LocationId);
        
        return new RemovePhotoResponse(locationResult.Value.Id);
    }
}