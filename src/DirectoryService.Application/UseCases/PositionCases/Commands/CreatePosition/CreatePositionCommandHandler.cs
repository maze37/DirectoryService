using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Abstractions.Database;
using DirectoryService.Application.Validation;
using DirectoryService.Contracts.Constants;
using DirectoryService.Contracts.PositionContracts;
using DirectoryService.Domain.DepartmentPositions;
using DirectoryService.Domain.Position;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Shared.Core;
using Shared.Result;

namespace DirectoryService.Application.UseCases.PositionCases.Commands.CreatePosition;

public class CreatePositionCommandHandler : ICommandHandler<CreatePositionCommand, CreatePositionResponse>
{
    private readonly IPositionRepository _positionRepository;
    private readonly IDepartmentRepository _departmentRepository;
    private readonly ITransactionManager _transactionManager;
    private readonly IDateTimeProvider _dateTime;
    private readonly IValidator<CreatePositionCommand> _validator;
    private readonly ILogger<CreatePositionCommandHandler> _logger;

    public CreatePositionCommandHandler(
        IPositionRepository positionRepository,
        IDepartmentRepository departmentRepository,
        ITransactionManager transactionManager,
        IDateTimeProvider dateTime,
        IValidator<CreatePositionCommand> validator,
        ILogger<CreatePositionCommandHandler> logger)
    {
        _positionRepository = positionRepository;
        _departmentRepository = departmentRepository;
        _transactionManager = transactionManager;
        _dateTime = dateTime;
        _validator = validator;
        _logger = logger;
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
            _logger.LogWarning("Должность с названием {PositionName} уже существует", command.Request.Name);
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

        _positionRepository.Add(positionResult.Value);

        var saveResult = await _transactionManager.SaveChangesAsync(cancellationToken);
        if (saveResult.IsFailure)
        {
            var constraint = saveResult.Error.InvalidField ?? "";

            if (constraint.Contains(IndexConstants.Positions.Name))
                return Error.Conflict("position.name.taken", "Должность с таким названием уже существует");

            return saveResult.Error;
        }

        _logger.LogInformation("Должность {Name} создана", command.Request.Name);
        return new CreatePositionResponse(positionResult.Value.Id);
    }
}
