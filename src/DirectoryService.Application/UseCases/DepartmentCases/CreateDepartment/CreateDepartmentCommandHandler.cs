using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
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

namespace DirectoryService.Application.UseCases.DepartmentCases.CreateDepartment;

public class CreateDepartmentCommandHandler : ICommandHandler<CreateDepartmentCommand, CreateDepartmentResponse>
{
    private readonly IDepartmentRepository _departmentRepository;
    private readonly ILocationRepository _locationRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTimeProvider _dateTime;
    private readonly ILogger _logger;
    private readonly IValidator<CreateDepartmentCommand> _validator;

    public CreateDepartmentCommandHandler(
        IDepartmentRepository departmentRepository,
        ILocationRepository locationRepository,
        IUnitOfWork unitOfWork,
        IDateTimeProvider dateTime,
        ILogger logger,
        IValidator<CreateDepartmentCommand> validator)
    {
        _departmentRepository = departmentRepository ?? throw new ArgumentNullException(nameof(departmentRepository));
        _locationRepository = locationRepository ?? throw new ArgumentNullException(nameof(locationRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _dateTime = dateTime ?? throw new ArgumentNullException(nameof(dateTime));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _validator = validator ?? throw new ArgumentNullException(nameof(validator));
    }

    public async Task<Result<CreateDepartmentResponse, Error>> HandleAsync(
        CreateDepartmentCommand command,
        CancellationToken cancellationToken = default)
    {
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.ToError();
        }

        bool locationExists = await _locationRepository
            .AllExistAsync(command.Request.LocationIds, cancellationToken);
        if (!locationExists)
            return Errors.General.NotFound(name: "locations");

        var identifierExists = await _departmentRepository
            .ExistsByIdentifierAsync(command.Request.Identifier, cancellationToken);
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
                command.Request.Identifier,
                0,
                _dateTime.UtcNow,
                departmentLocations);
        }
        else
        {
            var parent = await _departmentRepository
                .GetByIdAsync(command.Request.ParentId.Value, cancellationToken);

            if (parent is null)
                return Errors.General.NotFound(name: "parent department");

            if (!parent.IsActive)
                return Error.Failure("department.parent.inactive", "Родительский отдел неактивен");

            departmentResult = Department.CreateChild(
                departmentId,
                command.Request.Name,
                command.Request.Identifier,
                parent,
                _dateTime.UtcNow,
                departmentLocations);
        }

        if (departmentResult.IsFailure)
            return departmentResult.Error;
        
        await _departmentRepository.AddAsync(departmentResult.Value, cancellationToken);
    
        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException pgEx)
        {
            _logger.Error("DbUpdateException: {Message}", pgEx.Message);
        
            if (pgEx.SqlState == PostgresErrorCodes.UniqueViolation)
                return Error.Conflict("department.identifier.taken", "Отдел с таким идентификатором уже существует");
        
            throw;
        }

        _logger.Information("Отдел {Name} создан", command.Request.Name);
        return new CreateDepartmentResponse(departmentResult.Value.Id);
    }
}
