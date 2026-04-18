using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Validation;
using DirectoryService.Contracts.LocationContracts;
using DirectoryService.Domain.Location;
using FluentValidation;
using Serilog;
using Shared.Core;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace DirectoryService.Application.UseCases.LocationCases.CreateLocation;

public class CreateLocationCommandHandler : ICommandHandler<CreateLocationCommand, CreateLocationResponse>
{
    private readonly ILocationRepository _locationRepository;
    private readonly IDateTimeProvider _date;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;
    private readonly IValidator<CreateLocationCommand> _validator;

    public CreateLocationCommandHandler(
        ILocationRepository locationRepository,
        IDateTimeProvider date,
        IUnitOfWork unitOfWork,
        ILogger logger,
        IValidator<CreateLocationCommand> validator)
    {
        _locationRepository = locationRepository ?? throw new ArgumentNullException(nameof(locationRepository));
        _date = date ?? throw new ArgumentNullException(nameof(date));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentException(nameof(logger));
        _validator = validator ?? throw new ArgumentNullException(nameof(validator));
    }

    public async Task<Result<CreateLocationResponse, Error>> HandleAsync(
        CreateLocationCommand command,
        CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);

        if (!validationResult.IsValid)
        {
            return validationResult.ToError();
        }
        
        var locationResult = Location.Create(
            Guid.NewGuid(),
            command.Request.Name,
            command.Request.Address,
            command.Request.Timezone,
            _date.UtcNow);

        if (locationResult.IsFailure)
            return locationResult.Error;

        _logger.Information("Локация с названием: {LocationName} успешно создана", locationResult.Value.Name);

        await _locationRepository.AddAsync(locationResult.Value, cancellationToken);
        
        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException pgEx)
        {
            _logger.Error("DbUpdateException: {Message}", pgEx.Message);

            if (pgEx.SqlState == PostgresErrorCodes.UniqueViolation)
            {
                if (pgEx.ConstraintName?.Contains("name") == true)
                    return Error.Conflict("location.name.taken", "Локация с таким названием уже существует");

                if (pgEx.ConstraintName?.Contains("address") == true)
                    return Error.Conflict("location.address.taken", "Локация с таким адресом уже существует");
            }

            throw;
        }
        
        return new CreateLocationResponse(locationResult.Value.Id);
    }
}