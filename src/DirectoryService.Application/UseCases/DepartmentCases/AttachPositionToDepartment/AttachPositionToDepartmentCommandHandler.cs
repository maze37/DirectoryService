using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Validation;
using DirectoryService.Contracts.DepartmentContracts;
using DirectoryService.Domain.DepartmentPositions;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Shared.Core;

namespace DirectoryService.Application.UseCases.DepartmentCases.AttachPositionToDepartment;

public class AttachPositionToDepartmentCommandHandler : 
    ICommandHandler<AttachPositionToDepartmentCommand, AttachPositionToDepartmentResponse>
{
    private readonly IDepartmentRepository _departmentRepository;
    private readonly ITransactionManager _transactionManager;
    private readonly IDateTimeProvider _dateTime;
    private readonly ILogger<AttachPositionToDepartmentCommandHandler> _logger;
    private readonly IValidator<AttachPositionToDepartmentCommand> _validator;

    public AttachPositionToDepartmentCommandHandler(
        IDepartmentRepository departmentRepository,
        ITransactionManager transactionManager,
        IDateTimeProvider dateTime,
        ILogger<AttachPositionToDepartmentCommandHandler> logger,
        IValidator<AttachPositionToDepartmentCommand> validator)
    {
        _departmentRepository = departmentRepository;
        _transactionManager = transactionManager;
        _dateTime = dateTime;
        _logger = logger;
        _validator = validator;
    }
    
    public async Task<Result<AttachPositionToDepartmentResponse, Error>> HandleAsync(
        AttachPositionToDepartmentCommand command,
        CancellationToken cancellationToken = default)
    {
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
            return validationResult.ToError();

        var transactionResult = await _transactionManager.BeginTransactionAsync(cancellationToken);
        if (transactionResult.IsFailure)
            return transactionResult.Error;

        using var transaction = transactionResult.Value;

        var departmentResult = await _departmentRepository.GetByIdWithLock(
            command.DepartmentId, cancellationToken);
        if (departmentResult.IsFailure)
            return departmentResult.Error;

        if (!departmentResult.Value.IsActive)
            return Error.Failure("department.inactive", "Подразделение неактивно");

        var alreadyLinked = await _departmentRepository
            .IsPositionLinkedAsync(command.DepartmentId, command.PositionId, cancellationToken);
        if (alreadyLinked)
            return Error.Conflict("position.already.linked", "Должность уже привязана к подразделению.");

        var link = new DepartmentPosition(command.PositionId, command.DepartmentId);
        _departmentRepository.AddPositionLink(link);

        var saveResult = await _transactionManager.SaveChangesAsync(cancellationToken);
        if (saveResult.IsFailure)
        {
            transaction.Rollback();
            return saveResult.Error;
        }

        var commitResult = transaction.Commit();
        if (commitResult.IsFailure)
            return commitResult.Error;

        _logger.LogInformation("Должность {PositionId} привязана к подразделению {DepartmentId}",
            command.PositionId, command.DepartmentId);

        return new AttachPositionToDepartmentResponse(command.DepartmentId, command.PositionId);
    }
}