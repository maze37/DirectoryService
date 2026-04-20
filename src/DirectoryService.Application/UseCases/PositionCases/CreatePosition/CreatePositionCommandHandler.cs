using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Validation;
using DirectoryService.Contracts.PositionContracts;
using DirectoryService.Domain.DepartmentPositions;
using DirectoryService.Domain.Position;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Serilog;
using Shared.Core;
using Shared.Result;

namespace DirectoryService.Application.UseCases.PositionCases.CreatePosition;

public class CreatePositionCommandHandler : ICommandHandler<CreatePositionCommand, CreatePositionResponse>
{
    private readonly IPositionRepository _positionRepository;
    private readonly IDepartmentRepository _departmentRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTimeProvider _dateTime;
    private readonly IValidator<CreatePositionCommand> _validator;
    private readonly ILogger _logger;

    public CreatePositionCommandHandler(
        IPositionRepository positionRepository,
        IDepartmentRepository departmentRepository,
        IUnitOfWork unitOfWork,
        IDateTimeProvider dateTime,
        IValidator<CreatePositionCommand> validator,
        ILogger logger)
    {
        _positionRepository = positionRepository ?? throw new ArgumentNullException(nameof(positionRepository));
        _departmentRepository = departmentRepository ?? throw new ArgumentNullException(nameof(departmentRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _dateTime = dateTime ?? throw new ArgumentNullException(nameof(dateTime));
        _validator = validator ?? throw new ArgumentNullException(nameof(validator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<CreatePositionResponse, Error>> HandleAsync(
        CreatePositionCommand command,
        CancellationToken cancellationToken = default)
    {
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.ToError();
        }

        bool isExists = await _positionRepository
            .ExistsActiveWithNameAsync(command.Request.Name, cancellationToken);
        if (isExists)
        {
            _logger.Warning("Должность с названием {PositionName}", command.Request.Name);
            return Error.Conflict("position.name.taken", "Должность с таким названием уже существует");
        }

        var departmentExists = await _departmentRepository
            .AllExistAndActiveAsync(command.Request.DepartmentIds, cancellationToken);
        if (!departmentExists)
            return Errors.General.NotFound(name: "department");

        var positionId = Guid.NewGuid();
        var departmentPositions = command.Request.DepartmentIds
            .Select(departmentId => new DepartmentPosition(positionId, departmentId))
            .ToList();
        
        var positionResult = Position.Create(
            positionId,
            command.Request.Name,
            command.Request.Description,
            _dateTime.UtcNow,
            departmentPositions);

        if (positionResult.IsFailure)
            return positionResult.Error;

        await _positionRepository.AddAsync(positionResult.Value, cancellationToken);

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException pgEx)
        {
            _logger.Error("DbUpdateException: {Message}", pgEx.Message);
    
            if (pgEx.SqlState == PostgresErrorCodes.UniqueViolation)
                return Error.Conflict("position.name.taken", "Должность с таким названием уже существует");
    
            throw;
        }

        _logger.Information("Должность {Name} создана", command.Request.Name);
        return new CreatePositionResponse(positionResult.Value.Id);
    }
}