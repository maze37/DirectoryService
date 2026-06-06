using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Validation;
using DirectoryService.Contracts.Constants;
using DirectoryService.Contracts.LocationContracts;
using DirectoryService.Domain.Location;
using FluentValidation;
using Serilog;
using Shared.Core;

namespace DirectoryService.Application.UseCases.LocationCases.CreateLocation;

public class CreateLocationCommandHandler : ICommandHandler<CreateLocationCommand, CreateLocationResponse>
{
    private readonly ILocationRepository _locationRepository;
    private readonly IDateTimeProvider _date;
    private readonly ITransactionManager _transactionManager;
    private readonly ILogger _logger;
    private readonly IValidator<CreateLocationCommand> _validator;

    public CreateLocationCommandHandler(
        ILocationRepository locationRepository,
        IDateTimeProvider date,
        ITransactionManager transactionManager,
        ILogger logger,
        IValidator<CreateLocationCommand> validator)
    {
        _locationRepository = locationRepository;
        _date = date;
        _transactionManager = transactionManager;
        _logger = logger;
        _validator = validator;
    }

    public async Task<Result<CreateLocationResponse, Error>> HandleAsync(
        CreateLocationCommand command,
        CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
            return validationResult.ToError();
        
        var locationResult = Location.Create(
            Guid.NewGuid(),
            command.Request.Name,
            command.Request.Address,
            command.Request.Timezone,
            _date.UtcNow);

        if (locationResult.IsFailure)
            return locationResult.Error;

        _locationRepository.Add(locationResult.Value);
        
        var saveResult = await _transactionManager.SaveChangesAsync(cancellationToken);
        if (saveResult.IsFailure)
        {
            var constraint = saveResult.Error.InvalidField ?? "";
        
            if (constraint.Contains(IndexConstants.Locations.Name))
                return Error.Conflict("location.name.taken", "Локация с таким названием уже существует");
        
            if (constraint.Contains(IndexConstants.Locations.Address))
                return Error.Conflict("location.address.taken", "Локация с таким адресом уже существует");
        
            return saveResult.Error;
        }

        _logger.Information("Локация {Name} создана", locationResult.Value.Name.Value);
        return new CreateLocationResponse(locationResult.Value.Id);
    }
}
