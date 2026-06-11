using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Validation;
using DirectoryService.Contracts.DepartmentContracts;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Shared.Core;

namespace DirectoryService.Application.UseCases.DepartmentCases.MoveDepartment;

public class MoveDepartmentCommandHandler : ICommandHandler<MoveDepartmentCommand, MoveDepartmentResponse>
{
    private readonly IDepartmentRepository _departmentRepository;
    private readonly ITransactionManager _transactionManager;
    private readonly IDateTimeProvider _dateTime;
    private readonly ILogger<MoveDepartmentCommandHandler> _logger;
    private readonly IValidator<MoveDepartmentCommand> _validator;

    public MoveDepartmentCommandHandler(
        IDepartmentRepository departmentRepository,
        ITransactionManager transactionManager,
        IDateTimeProvider dateTime,
        ILogger<MoveDepartmentCommandHandler> logger,
        IValidator<MoveDepartmentCommand> validator)
    {
        _departmentRepository = departmentRepository;
        _transactionManager = transactionManager;
        _dateTime = dateTime;
        _logger = logger;
        _validator = validator;
    }

    public async Task<Result<MoveDepartmentResponse, Error>> HandleAsync(
    MoveDepartmentCommand command,
    CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
            return validationResult.ToError();

        var transactionScopeResult = await _transactionManager.BeginTransactionAsync(cancellationToken);
        if (transactionScopeResult.IsFailure)
            return transactionScopeResult.Error;

        using var transactionScope = transactionScopeResult.Value;

        // 1. Получаем department с блокировкой
        var departmentResult = await _departmentRepository.GetByIdWithLock(
            command.DepartmentId, cancellationToken);
        if (departmentResult.IsFailure)
            return departmentResult.Error;

        if (!departmentResult.Value.IsActive)
            return Error.Failure("department.inactive", "Отдел неактивен");

        var department = departmentResult.Value;
        string newParentPath = "";
        Guid? newParentId = null;

        // 2. Если parentId указан — проверяем его
        if (command.Request.ParentId != null)
        {
            if (command.DepartmentId == command.Request.ParentId)
                return Error.Failure("department.self.parent", "Нельзя выбрать себя родителем.");

            var parentResult = await _departmentRepository.GetByIdWithLock(
                command.Request.ParentId.Value, cancellationToken);
            if (parentResult.IsFailure)
                return parentResult.Error;

            if (!parentResult.Value.IsActive)
                return Error.Failure("parent.department.inactive", "Родительский отдел неактивен");

            // 3. Проверяем зацикливание
            var isDescendant = await _departmentRepository.IsDescendantOrSelfAsync(
                parentResult.Value.Path, department.Path, cancellationToken);
            if (isDescendant)
                return Error.Failure("department.cycle", "Нельзя выбрать потомка родителем.");

            newParentPath = parentResult.Value.Path;
            newParentId = parentResult.Value.Id;
        }

        // 4. Блокируем всех потомков
        await _departmentRepository.LockDescendantsAsync(department.Path, cancellationToken);

        // 5. Перемещаем
        await _departmentRepository.MoveDepartmentAsync(
            department.Path, newParentPath, department.Id, newParentId, cancellationToken);

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

        _logger.LogInformation("Подразделение {DepartmentId} перенесено", department.Id);

        return new MoveDepartmentResponse(department.Id);
    }
}