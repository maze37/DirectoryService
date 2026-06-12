using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Abstractions.Database;
using DirectoryService.Application.Validation;
using DirectoryService.Contracts.DepartmentContracts;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Shared.Core;

namespace DirectoryService.Application.UseCases.DepartmentCases.Commands.DetachPositionFromDepartment;

public class DetachPositionFromDepartmentCommandHandler : 
    ICommandHandler<DetachPositionFromDepartmentCommand, DetachPositionFromDepartmentResponse>
{
    private readonly IDepartmentRepository _departmentRepository;
    private readonly ITransactionManager _transactionManager;
    private readonly IDateTimeProvider _dateTime;
    private readonly ILogger<DetachPositionFromDepartmentCommandHandler> _logger;
    private readonly IValidator<DetachPositionFromDepartmentCommand> _validator;

    public DetachPositionFromDepartmentCommandHandler(
        IDepartmentRepository departmentRepository,
        ITransactionManager transactionManager,
        IDateTimeProvider dateTime,
        ILogger<DetachPositionFromDepartmentCommandHandler> logger,
        IValidator<DetachPositionFromDepartmentCommand> validator)
    {
        _departmentRepository = departmentRepository;
        _transactionManager = transactionManager;
        _dateTime = dateTime;
        _logger = logger;
        _validator = validator;
    }
    
    public async Task<Result<DetachPositionFromDepartmentResponse, Error>> HandleAsync(
        DetachPositionFromDepartmentCommand command,
        CancellationToken cancellationToken = default)
    {
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
            return validationResult.ToError();

        var transactionResult = await _transactionManager.BeginTransactionAsync(cancellationToken);
        if (transactionResult.IsFailure)
            return transactionResult.Error;

        using var transaction = transactionResult.Value;

        var linkResult = await _departmentRepository
            .GetPositionLinkAsync(command.DepartmentId, command.PositionId, cancellationToken);
        if (linkResult.IsFailure)
            return Error.NotFound("position.link.notfound", "Привязка должности к подразделению не найдена.");

        _departmentRepository.RemovePositionLink(linkResult.Value);

        var saveResult = await _transactionManager.SaveChangesAsync(cancellationToken);
        if (saveResult.IsFailure)
        {
            transaction.Rollback();
            return saveResult.Error;
        }

        var commitResult = transaction.Commit();
        if (commitResult.IsFailure)
            return commitResult.Error;

        _logger.LogInformation("Должность {PositionId} отвязана от подразделения {DepartmentId}",
            command.PositionId, command.DepartmentId);

        return new DetachPositionFromDepartmentResponse(command.DepartmentId, command.PositionId);
    }
}