using Core.Abstractions;
using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Abstractions.Database;
using DirectoryService.Application.Validation;
using DirectoryService.Contracts.DepartmentContracts;
using DirectoryService.Domain.Department;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Shared.Result;
using IDateTimeProvider = DirectoryService.Application.Abstractions.IDateTimeProvider;

namespace DirectoryService.Application.UseCases.DepartmentCases.Commands.MoveDepartment;

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
        // 1. Валидация команды
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
            return validationResult.ToError();

        // 2. Открываем транзакцию
        var transactionScopeResult = await _transactionManager.BeginTransactionAsync(cancellationToken);
        if (transactionScopeResult.IsFailure)
            return transactionScopeResult.Error;

        using var transactionScope = transactionScopeResult.Value;

        // 3. Получаем перемещаемый отдел с блокировкой
        var departmentResult = await _departmentRepository.GetByIdWithLock(
            command.DepartmentId, cancellationToken);
        if (departmentResult.IsFailure)
            return departmentResult.Error;

        var department = departmentResult.Value;

        if (!department.IsActive)
            return Error.Failure("department.inactive", "Отдел неактивен");

        // 4. Получаем нового родителя с блокировкой (если указан)
        Department? newParent = null;

        if (command.Request.ParentId is { } newParentId)
        {
            if (command.DepartmentId == newParentId)
                return Error.Conflict("department.self.parent", "Нельзя выбрать себя родителем.");

            var newParentResult = await _departmentRepository.GetByIdWithLock(
                newParentId, cancellationToken);
            if (newParentResult.IsFailure)
                return newParentResult.Error;

            newParent = newParentResult.Value;

            if (!newParent.IsActive)
                return Error.Conflict("parent.department.inactive", "Родительский отдел неактивен");

            // 5. Проверяем зацикливание
            var isDescendant = await _departmentRepository.IsDescendantOrSelfAsync(
                newParent.Path, department.Path, cancellationToken);
            if (isDescendant)
                return Error.Conflict("department.cycle", "Нельзя выбрать потомка родителем.");
        }

        // 6. Получаем старого родителя с блокировкой (до мутации)
        Department? oldParent = null;

        if (department.ParentId is { } oldParentId)
        {
            var oldParentResult = await _departmentRepository.GetByIdWithLock(
                oldParentId, cancellationToken);
            if (oldParentResult.IsFailure)
                return oldParentResult.Error;

            oldParent = oldParentResult.Value;
        }

        // 7. Блокируем потомков и перемещаем
        await _departmentRepository.LockDescendantsAsync(department.Path, cancellationToken);

        await _departmentRepository.MoveDepartmentAsync(
            department.Path,
            newParent?.Path ?? string.Empty,
            department.Id,
            newParent?.Id,
            cancellationToken);

        // 8. Обновляем счётчики детей
        newParent?.IncrementChildrenCount(_dateTime.UtcNow);
        oldParent?.DecrementChildrenCount(_dateTime.UtcNow);

        // 9. Сохраняем и коммитим
        var saveResult = await _transactionManager.SaveChangesAsync(cancellationToken);
        if (saveResult.IsFailure)
            return saveResult.Error; // Dispose transactionScope сделает rollback

        var commitResult = transactionScope.Commit();
        if (commitResult.IsFailure)
            return commitResult.Error;

        _logger.LogInformation("Подразделение {DepartmentId} перенесено в {NewParentId}",
            department.Id, newParent?.Id);

        return new MoveDepartmentResponse(department.Id);
    }
}