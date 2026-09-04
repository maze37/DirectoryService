using Core.Abstractions;
using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Abstractions.Database;
using DirectoryService.Application.Validation;
using DirectoryService.Contracts.PositionContracts;
using DirectoryService.Domain.Position.ValueObjects;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Shared.Result;
using IDateTimeProvider = DirectoryService.Application.Abstractions.IDateTimeProvider;

namespace DirectoryService.Application.UseCases.PositionCases.Commands.RenamePosition;

public class RenamePositionCommandHandler : ICommandHandler<RenamePositionCommand, RenamePositionResponse>
{
    private readonly IPositionRepository _positionRepository;
    private readonly ITransactionManager _transactionManager;
    private readonly IDateTimeProvider _dateTime;
    private readonly IValidator<RenamePositionCommand> _validator;
    private readonly ILogger<RenamePositionCommandHandler> _logger;

    public RenamePositionCommandHandler(
        IPositionRepository positionRepository,
        ITransactionManager transactionManager,
        IDateTimeProvider dateTime,
        IValidator<RenamePositionCommand> validator,
        ILogger<RenamePositionCommandHandler> logger)
    {
        _positionRepository = positionRepository;
        _transactionManager = transactionManager;
        _dateTime = dateTime;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result<RenamePositionResponse, Error>> HandleAsync(
        RenamePositionCommand command,
        CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.ToError();
        }
        
        var transactionScopeResult = await _transactionManager.BeginTransactionAsync(cancellationToken);
        if (transactionScopeResult.IsFailure)
        {
            return transactionScopeResult.Error;
        }

        using var transactionScope = transactionScopeResult.Value;
        
        // Получаем Position
        var positionResult = await _positionRepository.GetByIdWithLock(command.Id, cancellationToken);
        if (positionResult.IsFailure)
        {
            transactionScope.Rollback();
            return positionResult.Error;
        }
        
        var position = positionResult.Value;
        var dateTime = _dateTime.UtcNow;
        
        var newName = PositionName.Create(command.Request.Name);
        if (newName.IsFailure)
        {
            return newName.Error;
        }
        
        position.Rename(newName.Value, dateTime);
        _positionRepository.Update(position);
        
        // 6. Сохраняем
        var saveResult = await _transactionManager.SaveChangesAsync(cancellationToken);
        if (saveResult.IsFailure)
        {
            transactionScope.Rollback();
            return saveResult.Error;
        }
        
        var commitResult = transactionScope.Commit();
        if (commitResult.IsFailure)
            return commitResult.Error;

        _logger.LogInformation("Позиция {PositionId} переименована", position.Id);

        return new RenamePositionResponse(position.Id);
    }
}