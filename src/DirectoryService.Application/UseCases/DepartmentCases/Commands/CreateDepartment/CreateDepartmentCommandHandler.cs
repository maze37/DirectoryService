using Core.Abstractions;
using Core.Validation;
using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Abstractions.Database;
using DirectoryService.Contracts.DepartmentContracts;
using DirectoryService.Domain.Department;
using DirectoryService.Domain.DepartmentLocations;
using FluentValidation;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;
using SharedKernel;
using IDateTimeProvider = DirectoryService.Application.Abstractions.IDateTimeProvider;
using ILogger = Serilog.ILogger;

namespace DirectoryService.Application.UseCases.DepartmentCases.Commands.CreateDepartment;

public class CreateDepartmentCommandHandler : ICommandHandler<CreateDepartmentCommand, CreateDepartmentResponse>
{
    private readonly IDepartmentRepository _departmentRepository;
    private readonly ILocationRepository _locationRepository;
    private readonly ITransactionManager _transactionManager;
    private readonly IDateTimeProvider _dateTime;
    private readonly ILogger<CreateDepartmentCommandHandler> _logger;
    private readonly IValidator<CreateDepartmentCommand> _validator;
    private readonly HybridCache _cache;

    public CreateDepartmentCommandHandler(
        IDepartmentRepository departmentRepository,
        ILocationRepository locationRepository,
        ITransactionManager transactionManager,
        IDateTimeProvider dateTime,
        ILogger<CreateDepartmentCommandHandler> logger,
        IValidator<CreateDepartmentCommand> validator,
        HybridCache cache)
    {
        _departmentRepository = departmentRepository;
        _locationRepository = locationRepository;
        _transactionManager = transactionManager;
        _dateTime = dateTime;
        _logger = logger;
        _validator = validator;
        _cache = cache;
    }

    public async Task<Result<CreateDepartmentResponse, Error>> HandleAsync(
        CreateDepartmentCommand command,
        CancellationToken cancellationToken = default)
    {
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);
        if (validationResult.IsValid == false)
            return validationResult.ToError();

        var transaction = await _transactionManager.BeginTransactionAsync(cancellationToken);
        if (transaction.IsFailure)
            return transaction.Error;

        bool locationExists = await _locationRepository
            .AllExistAsync(command.Request.LocationIds, cancellationToken);
        if (!locationExists)
            return GeneralErrors.NotFound(null, "locations");

        // Для несуществующих строк в бд FOR UPDATE не сработает.
        // Для красоты стоит, но есть уникальный индекс в бд который спасает от race condition.
        bool identifierExists = await _departmentRepository
            .ExistsBySlugWithLockAsync(command.Request.Slug, cancellationToken);
        if (identifierExists)
            return Error.Conflict("department.identifier.taken", "Отдел с таким идентификатором уже существует");

        var departmentId = Guid.NewGuid();
        var departmentLocations = command.Request.LocationIds
            .Select(locationId => new DepartmentLocation(departmentId, locationId))
            .ToList();

        Result<Department, Error> departmentResult;

        if (command.Request.ParentId == null)
        {
            departmentResult = Department.CreateRoot(
                departmentId,
                command.Request.Name,
                command.Request.Slug,
                depth: 0,
                _dateTime.UtcNow,
                departmentLocations);
        }
        else
        {
            var parentResult = await _departmentRepository.GetByAsync(
                department => department.Id == command.Request.ParentId.Value,
                cancellationToken);

            if (parentResult.IsFailure)
                return parentResult.Error;

            var parent = parentResult.Value;

            if (!parent.IsActive)
                return Error.Failure("department.parent.inactive", "Родительский отдел неактивен");
            
            parent.IncrementChildrenCount(_dateTime.UtcNow);

            departmentResult = Department.CreateChild(
                departmentId,
                command.Request.Name,
                command.Request.Slug,
                parent,
                _dateTime.UtcNow,
                departmentLocations);
        }

        if (departmentResult.IsFailure)
            return departmentResult.Error;
        
        _departmentRepository.Add(departmentResult.Value);
        
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
        
        _logger.LogInformation("Отдел {Name} создан", command.Request.Name);
        return new CreateDepartmentResponse(departmentResult.Value.Id);
    }
}
