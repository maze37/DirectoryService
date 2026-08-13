using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Abstractions.Database;
using DirectoryService.Application.Validation;
using DirectoryService.Contracts.PositionContracts;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Shared.Core;

namespace DirectoryService.Application.UseCases.PositionCases.Commands.RestorePosition;

public class RestorePositionCommandHandler : ICommandHandler<RestorePositionCommand, RestorePositionResponse>
{
    private readonly IPositionRepository _positionRepository;
    private readonly ITransactionManager _transactionManager;
    private readonly IValidator<RestorePositionCommand> _validator;
    private readonly ILogger<RestorePositionCommandHandler> _logger;
    private readonly IDateTimeProvider _dateTime;

    public RestorePositionCommandHandler(
        IPositionRepository positionRepository,
        ITransactionManager transactionManager,
        IValidator<RestorePositionCommand> validator,
        ILogger<RestorePositionCommandHandler> logger,
        IDateTimeProvider dateTime)
    {
        _positionRepository = positionRepository;
        _transactionManager = transactionManager;
        _validator = validator;
        _logger = logger;
        _dateTime = dateTime;
    }
    
    public async Task<Result<RestorePositionResponse, Error>> HandleAsync(
        RestorePositionCommand command,
        CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
            return validationResult.ToError();

        var transactionScopeResult = await _transactionManager.BeginTransactionAsync(cancellationToken);
        if (transactionScopeResult.IsFailure)
            return transactionScopeResult.Error;
        
        using var transactionScope = transactionScopeResult.Value;
        
        var positionResult = await _positionRepository.GetByIdWithLock(command.PositionId, cancellationToken);
        if (positionResult.IsFailure)
        {
            transactionScope.Rollback();
            return positionResult.Error;
        }
        
        var position = positionResult.Value;
        
        position.Restore();
        
        var saveResult = await _transactionManager.SaveChangesAsync(cancellationToken);
        if (saveResult.IsFailure)
        {
            transactionScope.Rollback();
            return saveResult.Error;
        }

        var commitResult = transactionScope.Commit();
        if (commitResult.IsFailure)
            return commitResult.Error;
        
        _logger.LogInformation("Должность {PositionId} восстановлено", position.Id);
        return new RestorePositionResponse(position.Id);
    }
}