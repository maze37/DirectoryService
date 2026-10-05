using Core.Abstractions;
using Core.Validation;
using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Abstractions.Database;
using DirectoryService.Contracts.DepartmentContracts;
using FluentValidation;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;
using SharedKernel;
using IDateTimeProvider = DirectoryService.Application.Abstractions.IDateTimeProvider;

namespace DirectoryService.Application.UseCases.DepartmentCases.Commands.DeleteDepartment;

public class DeleteDepartmentCommandHandler : ICommandHandler<DeleteDepartmentCommand, DeleteDepartmentResponse>
{
    private readonly IDepartmentRepository _departmentRepository;
    private readonly ITransactionManager _transactionManager;
    private readonly ILogger<DeleteDepartmentCommandHandler> _logger;
    private readonly IValidator<DeleteDepartmentCommand> _validator;
    private readonly IDateTimeProvider _dateTime;
    private readonly HybridCache _cache;

    public DeleteDepartmentCommandHandler(
        IDepartmentRepository departmentRepository,
        ITransactionManager transactionManager,
        ILogger<DeleteDepartmentCommandHandler> logger,
        IValidator<DeleteDepartmentCommand> validator,
        IDateTimeProvider dateTime,
        HybridCache cache)
    {
        _departmentRepository = departmentRepository;
        _transactionManager = transactionManager;
        _logger = logger;
        _validator = validator;
        _dateTime = dateTime;
        _cache = cache;
    }

    public async Task<Result<DeleteDepartmentResponse, Error>> HandleAsync(
        DeleteDepartmentCommand command,
        CancellationToken cancellationToken = default)
    {
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
            return validationResult.ToError();
    
        var transaction = await _transactionManager.BeginTransactionAsync(cancellationToken);
        if (transaction.IsFailure)
            return transaction.Error;

        var departmentResult = await _departmentRepository.GetByIdWithLock(command.Id, cancellationToken);
        if (departmentResult.IsFailure)
        {
            return departmentResult.Error;
        }

        if (departmentResult.Value.ChildrenCount > 0) 
        {
            return Error.Validation(
                "department.has.children", 
                "Нельзя удалить подразделение, у которого есть дочерние элементы. Сначала удалите их.");
        }
        
        departmentResult.Value.SoftDelete(_dateTime.UtcNow);

        var commitResult = await _transactionManager.CommitTransactionAsync(cancellationToken);
        if (commitResult.IsFailure)
            return commitResult.Error;
        
        try
        {
            await _cache.RemoveByTagAsync("departments-tree", cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Не удалось сбросить кэш дерева подразделений");
        }

        _logger.LogInformation("Подразделение {DepartmentId} удалено", command.Id);
        return new DeleteDepartmentResponse(command.Id);
    }
}