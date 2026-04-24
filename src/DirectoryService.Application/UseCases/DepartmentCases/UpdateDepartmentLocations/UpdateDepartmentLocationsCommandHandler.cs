using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Validation;
using DirectoryService.Contracts.DepartmentContracts;
using DirectoryService.Domain.DepartmentLocations;
using FluentValidation;
using Shared.Core;
using Shared.Result;
using ILogger = Serilog.ILogger;

namespace DirectoryService.Application.UseCases.DepartmentCases.UpdateDepartmentLocations;

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
        // 1. Валидация входящих данных
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
            return validationResult.ToError();
        
        // 2. Проверяем - существует ли подразделение и активно ли оно
        var departmentResult = await _departmentRepository.GetByAsync(
            d => d.Id == command.DepartmentId, cancellationToken);
        if (departmentResult.IsFailure)
            return departmentResult.Error;
        
        var department = departmentResult.Value;

        if (!department.IsActive)
            return Error.Failure("department.inactive", "Подразделение неактивно");

        // 3. Проверяем — все locationIds существуют и активны
        var allLocationsExist = await _locationRepository
            .AllExistAsync(command.Request.LocationIds, cancellationToken);
        
        if (!allLocationsExist)
            return Errors.General.NotFound(name: "locations");
        
        // 4. Формируем новый список привязок и обновляем
        var newLocations = command.Request.LocationIds
            .Select(locationId => new DepartmentLocation(department.Id, locationId))
            .ToList();
        
        department.UpdateLocations(newLocations, _dateTime.UtcNow);
        
        await _departmentRepository.UpdateLocationsAsync(department, cancellationToken);

        var saveResult = await _transactionManager.SaveChangesAsync(cancellationToken);
        if (saveResult.IsFailure)
            return saveResult.Error;
        
        _logger.Information(
            "Локации подразделения {DepartmentId} обновлены",
            department.Id);

        return new UpdateDepartmentLocationsResponse(department.Id);
    }
}