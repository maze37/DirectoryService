using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Abstractions.Database;
using DirectoryService.Application.Validation;
using DirectoryService.Contracts.DepartmentContracts;
using DirectoryService.Domain.Department;
using DirectoryService.Domain.DepartmentLocations;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Serilog;
using Shared.Core;
using Shared.Result;

namespace DirectoryService.Application.UseCases.DepartmentCases.Commands.CreateDepartment;

public class CreateDepartmentCommandHandler : ICommandHandler<CreateDepartmentCommand, CreateDepartmentResponse>
{
    private readonly IDepartmentRepository _departmentRepository;
    private readonly ILocationRepository _locationRepository;
    private readonly ITransactionManager _transactionManager;
    private readonly IDateTimeProvider _dateTime;
    private readonly ILogger _logger;
    private readonly IValidator<CreateDepartmentCommand> _validator;

    public CreateDepartmentCommandHandler(
        IDepartmentRepository departmentRepository,
        ILocationRepository locationRepository,
        ITransactionManager transactionManager,
        IDateTimeProvider dateTime,
        ILogger logger,
        IValidator<CreateDepartmentCommand> validator)
    {
        _departmentRepository = departmentRepository;
        _locationRepository = locationRepository;
        _transactionManager = transactionManager;
        _dateTime = dateTime;
        _logger = logger;
        _validator = validator;
    }

    public async Task<Result<CreateDepartmentResponse, Error>> HandleAsync(
        CreateDepartmentCommand command,
        CancellationToken cancellationToken = default)
    {
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);
        if (validationResult.IsValid == false)
            return validationResult.ToError();

        var transactionScopeResult = await _transactionManager.BeginTransactionAsync(cancellationToken);
        if (transactionScopeResult.IsFailure)
            return transactionScopeResult.Error;

        using var transactionScope = transactionScopeResult.Value;
        
        bool locationExists = await _locationRepository
            .AllExistAsync(command.Request.LocationIds, cancellationToken);
        if (!locationExists)
            return Errors.General.NotFound(name: "locations");

        // Для несуществующих строк в бд FOR UPDATE не сработает.
        // Для красоты стоит, но есть уникальный индекс в бд который спасает от race condition.
        var identifierExists = await _departmentRepository
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
        
        try 
        {
            var saveResult = await _transactionManager.SaveChangesAsync(cancellationToken); 
            if (saveResult.IsFailure) 
            { 
                transactionScope.Rollback(); 
                return saveResult.Error; 
            } 
        } 
        catch (DbUpdateException ex) when 
            (ex.InnerException is PostgresException postgres && 
             postgres.SqlState == PostgresErrorCodes.UniqueViolation && 
             postgres.ConstraintName == "ux_departments_slug") 
        { 
            transactionScope.Rollback(); 
            return Error.Conflict( "department.identifier.taken", 
                "Отдел с таким идентификатором уже существует"); 
        }

        var commitResult = transactionScope.Commit();
        if (commitResult.IsFailure)
            return commitResult.Error;
        
        _logger.Information("Отдел {Name} создан", command.Request.Name);
        return new CreateDepartmentResponse(departmentResult.Value.Id);
    }
}
