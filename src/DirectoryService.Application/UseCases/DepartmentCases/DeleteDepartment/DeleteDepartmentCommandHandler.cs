using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Validation;
using DirectoryService.Contracts.DepartmentContracts;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Shared.Core;

namespace DirectoryService.Application.UseCases.DepartmentCases.DeleteDepartment;

public class DeleteDepartmentCommandHandler : ICommandHandler<DeleteDepartmentCommand, DeleteDepartmentResponse>
{
    private readonly IDepartmentRepository _departmentRepository;
    private readonly ITransactionManager _transactionManager;
    private readonly ILogger<DeleteDepartmentCommandHandler> _logger;
    private readonly IValidator<DeleteDepartmentCommand> _validator;

    public DeleteDepartmentCommandHandler(
        IDepartmentRepository departmentRepository,
        ITransactionManager transactionManager,
        ILogger<DeleteDepartmentCommandHandler> logger,
        IValidator<DeleteDepartmentCommand> validator)
    {
        _departmentRepository = departmentRepository;
        _transactionManager = transactionManager;
        _logger = logger;
        _validator = validator;
    }

    public async Task<Result<DeleteDepartmentResponse, Error>> HandleAsync(
        DeleteDepartmentCommand command,
        CancellationToken cancellationToken = default)
    {
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
            return validationResult.ToError();
    
        var transactionResult = await _transactionManager.BeginTransactionAsync(cancellationToken);
        if (transactionResult.IsFailure)
            return transactionResult.Error;

        using var transaction = transactionResult.Value;

        var departmentResult = await _departmentRepository.GetByIdWithLock(command.Id, cancellationToken);
        if (departmentResult.IsFailure)
        {
            transaction.Rollback();
            return departmentResult.Error;
        }
        
        await _departmentRepository.DeleteWithDescendants(departmentResult.Value.Path, cancellationToken);

        var saveResult = await _transactionManager.SaveChangesAsync(cancellationToken);
        if (saveResult.IsFailure)
        {
            transaction.Rollback();
            return saveResult.Error;
        }

        var commitResult = transaction.Commit();
        if (commitResult.IsFailure)
            return commitResult.Error;

        _logger.LogInformation("Подразделение {DepartmentId} удалено", command.Id);
        return new DeleteDepartmentResponse(command.Id);
    }
}