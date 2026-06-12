using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Abstractions.Database;
using DirectoryService.Application.Validation;
using DirectoryService.Contracts.PositionContracts;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Shared.Core;

namespace DirectoryService.Application.UseCases.PositionCases.Commands.DeletePosition;

public class DeletePositionCommandHandler : ICommandHandler<DeletePositionCommand, DeletePositionResponse>
{
    private readonly IPositionRepository _positionRepository;
    private readonly ITransactionManager _transactionManager;
    private readonly IValidator<DeletePositionCommand> _validator;
    private readonly ILogger<DeletePositionCommandHandler> _logger;

    public DeletePositionCommandHandler(
        IPositionRepository positionRepository,
        ITransactionManager transactionManager,
        IValidator<DeletePositionCommand> validator,
        ILogger<DeletePositionCommandHandler> logger)
    {
        _positionRepository = positionRepository;
        _transactionManager = transactionManager;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result<DeletePositionResponse, Error>> HandleAsync(
        DeletePositionCommand command,
        CancellationToken cancellationToken = default)
    {
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.ToError();
        }

        var transactionResult = await _transactionManager.BeginTransactionAsync(cancellationToken);
        if (transactionResult.IsFailure)
        {
            return transactionResult.Error;
        }

        using var transaction = transactionResult.Value;

        var positionResult = await _positionRepository.GetByIdWithLock(command.Id, cancellationToken);
        if (positionResult.IsFailure)
        {
            transaction.Rollback();
            return positionResult.Error;
        }
        
        var hasLinks = await _positionRepository.HasDepartmentLinksAsync(command.Id, cancellationToken);
        if (hasLinks)
            return Error.Conflict("position.has.links", "Должность привязана к подразделениям. Сначала отвяжите её.");
        
        _positionRepository.Remove(positionResult.Value);
        
        var saveResult = await _transactionManager.SaveChangesAsync(cancellationToken);
        if (saveResult.IsFailure)
        {
            transaction.Rollback();
            return saveResult.Error;
        }

        var commitResult = transaction.Commit();
        if (commitResult.IsFailure)
            return commitResult.Error;

        _logger.LogInformation("Должность {PositionId} удалена", command.Id);
        return new DeletePositionResponse(command.Id);
    }
}