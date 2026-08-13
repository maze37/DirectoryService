using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Abstractions.Database;
using DirectoryService.Application.Validation;
using DirectoryService.Contracts.LocationContracts;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Shared.Core;

namespace DirectoryService.Application.UseCases.LocationCases.Commands.RestoreLocation;

public class RestoreLocationCommandHandler : ICommandHandler<RestoreLocationCommand, RestoreLocationResponse>
{
    private readonly ILocationRepository _locationRepository;
    private readonly ITransactionManager _transactionManager;
    private readonly ILogger<RestoreLocationCommandHandler> _logger;
    private readonly IValidator<RestoreLocationCommand> _validator;
    private readonly IDateTimeProvider _dateTime;

    public RestoreLocationCommandHandler(
        ILocationRepository locationRepository,
        ITransactionManager transactionManager,
        ILogger<RestoreLocationCommandHandler> logger,
        IValidator<RestoreLocationCommand> validator,
        IDateTimeProvider dateTime)
    {
        _locationRepository = locationRepository;
        _transactionManager = transactionManager;
        _logger = logger;
        _validator = validator;
        _dateTime = dateTime;
    }

    public async Task<Result<RestoreLocationResponse, Error>> HandleAsync(
        RestoreLocationCommand command,
        CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
            return validationResult.ToError();

        var transactionScopeResult = await _transactionManager.BeginTransactionAsync(cancellationToken);
        if (transactionScopeResult.IsFailure)
            return transactionScopeResult.Error;
        
        using var transactionScope = transactionScopeResult.Value;
        
        var locationResult = await _locationRepository.GetByIdWithLock(command.LocationId, cancellationToken);
        if (locationResult.IsFailure)
        {
            transactionScope.Rollback();
            return locationResult.Error;
        }
        
        var location = locationResult.Value;
        
        location.Restore();
        
        var saveResult = await _transactionManager.SaveChangesAsync(cancellationToken);
        if (saveResult.IsFailure)
        {
            transactionScope.Rollback();
            return saveResult.Error;
        }

        var commitResult = transactionScope.Commit();
        if (commitResult.IsFailure)
            return commitResult.Error;
        
        _logger.LogInformation("Локация {LocationId} восстановлено", location.Id);
        return new RestoreLocationResponse(location.Id);
    }
}