using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Validation;
using DirectoryService.Contracts.PositionContracts;
using DirectoryService.Domain.DepartmentPositions;
using DirectoryService.Domain.Position;
using FluentValidation;
using Serilog;
using Shared.Core;
using Shared.Result;

namespace DirectoryService.Application.UseCases.PositionCases.CreatePosition;

public class CreatePositionCommandHandler : ICommandHandler<CreatePositionCommand, CreatePositionResponse>
{
    private readonly IPositionRepository _positionRepository;
    private readonly IDepartmentRepository _departmentRepository;
    private readonly ITransactionManager _transactionManager;
    private readonly IDateTimeProvider _dateTime;
    private readonly IValidator<CreatePositionCommand> _validator;
    private readonly ILogger _logger;

    public CreatePositionCommandHandler(
        IPositionRepository positionRepository,
        IDepartmentRepository departmentRepository,
        ITransactionManager transactionManager,
        IDateTimeProvider dateTime,
        IValidator<CreatePositionCommand> validator,
        ILogger logger)
    {
        _positionRepository = positionRepository ?? throw new ArgumentNullException(nameof(positionRepository));
        _departmentRepository = departmentRepository ?? throw new ArgumentNullException(nameof(departmentRepository));
        _transactionManager = transactionManager ?? throw new ArgumentNullException(nameof(transactionManager));
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
            return validationResult.ToError();

        bool isExists = await _positionRepository
            .ExistsActiveWithNameAsync(command.Request.Name, cancellationToken);
        if (isExists)
        {
            _logger.Warning("Должность с названием {PositionName} уже существует", command.Request.Name);
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

        var saveResult = await _transactionManager.SaveChangesAsync(cancellationToken);
        if (saveResult.IsFailure)
        {
            var constraint = saveResult.Error.InvalidField ?? "";

            if (constraint.Contains("ix_positions_name"))
                return Error.Conflict("position.name.taken", "Должность с таким названием уже существует");

            return saveResult.Error;
        }

        _logger.Information("Должность {Name} создана", command.Request.Name);
        return new CreatePositionResponse(positionResult.Value.Id);
    }
}