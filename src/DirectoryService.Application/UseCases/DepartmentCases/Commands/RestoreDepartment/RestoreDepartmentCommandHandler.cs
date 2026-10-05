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

namespace DirectoryService.Application.UseCases.DepartmentCases.Commands.RestoreDepartment;

public class RestoreDepartmentCommandHandler : ICommandHandler<RestoreDepartmentCommand, RestoreDepartmentResponse>
{
    private readonly IDepartmentRepository _departmentRepository;
    private readonly ITransactionManager _transactionManager;
    private readonly ILogger<RestoreDepartmentCommandHandler> _logger;
    private readonly IValidator<RestoreDepartmentCommand> _validator;
    private readonly HybridCache _cache;

    public RestoreDepartmentCommandHandler(
        IDepartmentRepository departmentRepository,
        ITransactionManager transactionManager,
        ILogger<RestoreDepartmentCommandHandler> logger,
        IValidator<RestoreDepartmentCommand> validator,
        HybridCache cache)
    {
        _departmentRepository = departmentRepository;
        _transactionManager = transactionManager;
        _logger = logger;
        _validator = validator;
        _cache = cache;
    }
    
    public async Task<Result<RestoreDepartmentResponse, Error>> HandleAsync(
        RestoreDepartmentCommand command,
        CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
            return validationResult.ToError();

        var transaction = await _transactionManager.BeginTransactionAsync(cancellationToken);
        if (transaction.IsFailure)
            return transaction.Error;

        var departmentResult = await _departmentRepository.GetDeletedByIdWithLock(command.DepartmentId, cancellationToken);
        if (departmentResult.IsFailure)
        {
            return departmentResult.Error;
        }
        
        var department = departmentResult.Value;
        
        department.Restore();
        
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
        
        _logger.LogInformation("Подразделение {DepartmentId} восстановлено", department.Id);
        return new RestoreDepartmentResponse(department.Id);
    }
}