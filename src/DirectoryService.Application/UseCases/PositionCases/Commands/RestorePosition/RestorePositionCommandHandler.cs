using Core.Abstractions;
using Core.Validation;
using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Abstractions.Database;
using DirectoryService.Contracts.PositionContracts;
using FluentValidation;
using Microsoft.Extensions.Logging;
using SharedKernel;
using IDateTimeProvider = DirectoryService.Application.Abstractions.IDateTimeProvider;

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

        var transaction = await _transactionManager.BeginTransactionAsync(cancellationToken);
        if (transaction.IsFailure)
            return transaction.Error;

        var positionResult = await _positionRepository.GetDeletedByIdWithLock(command.PositionId, cancellationToken);
        if (positionResult.IsFailure)
        {
            return positionResult.Error;
        }
        
        var position = positionResult.Value;
        
        position.Restore();
        
        var commitResult = await _transactionManager.CommitTransactionAsync(cancellationToken);
        if (commitResult.IsFailure)
            return commitResult.Error;
        
        _logger.LogInformation("Должность {PositionId} восстановлено", position.Id);
        return new RestorePositionResponse(position.Id);
    }
}