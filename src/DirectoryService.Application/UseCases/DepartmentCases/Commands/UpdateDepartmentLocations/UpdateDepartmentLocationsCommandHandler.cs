using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Abstractions.Database;
using DirectoryService.Application.Validation;
using DirectoryService.Contracts.DepartmentContracts;
using DirectoryService.Domain.DepartmentLocations;
using FluentValidation;
using Shared.Core;
using Shared.Result;
using ILogger = Serilog.ILogger;

namespace DirectoryService.Application.UseCases.DepartmentCases.Commands.UpdateDepartmentLocations;

public class UpdateDepartmentLocationsCommandHandler : 
    ICommandHandler<UpdateDepartmentLocationsCommand, UpdateDepartmentLocationsResponse>
{
    private readonly ILocationRepository _locationRepository;
    private readonly IDepartmentRepository _departmentRepository;
    private readonly ITransactionManager _transactionManager;
    private readonly IDateTimeProvider _dateTime;
    private readonly ILogger _logger;
    private readonly IValidator<UpdateDepartmentLocationsCommand> _validator;
    
    public UpdateDepartmentLocationsCommandHandler(
        IDepartmentRepository departmentRepository,
        ILocationRepository locationRepository,
        ITransactionManager transactionManager,
        IDateTimeProvider dateTime,
        ILogger logger,
        IValidator<UpdateDepartmentLocationsCommand> validator)
    {
        _departmentRepository = departmentRepository;
        _locationRepository = locationRepository;
        _transactionManager = transactionManager;
        _dateTime = dateTime;
        _logger = logger;
        _validator = validator;
    }

    public async Task<Result<UpdateDepartmentLocationsResponse, Error>> HandleAsync(
        UpdateDepartmentLocationsCommand command,
        CancellationToken cancellationToken = default)
    {
        // Валидация входящих данных
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
            return validationResult.ToError();

        // Открываем транзакцию
        var transactionScopeResult = await _transactionManager.BeginTransactionAsync(cancellationToken);
        if (transactionScopeResult.IsFailure)
        {
            return transactionScopeResult.Error;
        }

        // using - вызвать dispose, тк IDispose есть у TransactionScope
        using var transactionScope = transactionScopeResult.Value;
        
        // Проверяем - существует ли подразделение
        // Песеммистичная блокировка - А должен коммитнуть, только потом возьмется за свежие данные B
        var departmentResult = await _departmentRepository.GetByIdWithLock(
            command.DepartmentId, cancellationToken);
        if (departmentResult.IsFailure)
        {
            transactionScope.Rollback();
            return departmentResult.Error;
        }
        
        var department = departmentResult.Value;
        if (!department.IsActive)
            return Error.Failure("department.inactive", "Подразделение неактивно");

        // Проверяем, что все locationIds существуют и активны
        var allLocationsExist = await _locationRepository
            .AllExistAsync(command.Request.LocationIds, cancellationToken);
        if (!allLocationsExist)
            return Errors.General.NotFound(name: "locations");
        
        // Формируем новый список привязок и обновляем
        var newLocations = command.Request.LocationIds
            .Select(locationId => new DepartmentLocation(department.Id, locationId))
            .ToList();
        
        department.UpdateLocations(newLocations, _dateTime.UtcNow);
        
        await _departmentRepository.UpdateLocationsAsync(department, cancellationToken);

        var saveResult = await _transactionManager.SaveChangesAsync(cancellationToken);
        if (saveResult.IsFailure)
        {
            transactionScope.Rollback();
            return saveResult.Error;
        }
        
        // Закрываем транзакцию и параллельно проверяем успешность сохранения
        var commitedResult = transactionScope.Commit();
        if (commitedResult.IsFailure)
        {
            return commitedResult.Error;
        }
        
        _logger.Information(
            "Локации подразделения {DepartmentId} обновлены",
            department.Id);

        return new UpdateDepartmentLocationsResponse(department.Id);
    }
}