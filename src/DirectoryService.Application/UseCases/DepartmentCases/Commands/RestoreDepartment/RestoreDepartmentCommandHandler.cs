using Core.Abstractions;
using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Abstractions.Database;
using DirectoryService.Application.Validation;
using DirectoryService.Contracts.DepartmentContracts;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Shared.Result;
using IDateTimeProvider = DirectoryService.Application.Abstractions.IDateTimeProvider;

namespace DirectoryService.Application.UseCases.DepartmentCases.Commands.RestoreDepartment;

public class RestoreDepartmentCommandHandler : ICommandHandler<RestoreDepartmentCommand, RestoreDepartmentResponse>
{
    private readonly IDepartmentRepository _departmentRepository;
    private readonly ITransactionManager _transactionManager;
    private readonly IDateTimeProvider _dateTime;
    private readonly ILogger<RestoreDepartmentCommandHandler> _logger;
    private readonly IValidator<RestoreDepartmentCommand> _validator;

    public RestoreDepartmentCommandHandler(
        IDepartmentRepository departmentRepository,
        ITransactionManager transactionManager,
        IDateTimeProvider dateTime,
        ILogger<RestoreDepartmentCommandHandler> logger,
        IValidator<RestoreDepartmentCommand> validator)
    {
        _departmentRepository = departmentRepository;
        _transactionManager = transactionManager;
        _dateTime = dateTime;
        _logger = logger;
        _validator = validator;
    }
    
    public async Task<Result<RestoreDepartmentResponse, Error>> HandleAsync(
        RestoreDepartmentCommand command,
        CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
            return validationResult.ToError();

        var transactionScopeResult = await _transactionManager.BeginTransactionAsync(cancellationToken);
        if (transactionScopeResult.IsFailure)
            return transactionScopeResult.Error;
        
        using var transactionScope = transactionScopeResult.Value;
        
        var departmentResult = await _departmentRepository.GetDeletedByIdWithLock(command.DepartmentId, cancellationToken);
        if (departmentResult.IsFailure)
        {
            transactionScope.Rollback();
            return departmentResult.Error;
        }
        
        var department = departmentResult.Value;
        
        department.Restore();
        
        var saveResult = await _transactionManager.SaveChangesAsync(cancellationToken);
        if (saveResult.IsFailure)
        {
            transactionScope.Rollback();
            return saveResult.Error;
        }

        var commitResult = transactionScope.Commit();
        if (commitResult.IsFailure)
            return commitResult.Error;
        
        _logger.LogInformation("Подразделение {DepartmentId} восстановлено", department.Id);
        return new RestoreDepartmentResponse(department.Id);
    }
}