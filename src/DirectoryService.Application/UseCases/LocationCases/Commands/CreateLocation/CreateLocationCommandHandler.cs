using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Abstractions.Database;
using DirectoryService.Application.Validation;
using DirectoryService.Contracts.Constants;
using DirectoryService.Contracts.LocationContracts;
using DirectoryService.Domain.Location;
using DirectoryService.Domain.Location.ValueObjects;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Shared.Core;

namespace DirectoryService.Application.UseCases.LocationCases.Commands.CreateLocation;

public class CreateLocationCommandHandler : ICommandHandler<CreateLocationCommand, CreateLocationResponse>
{
    private readonly ILocationRepository _locationRepository;
    private readonly IDateTimeProvider _date;
    private readonly ITransactionManager _transactionManager;
    private readonly ILogger<CreateLocationCommandHandler> _logger;
    private readonly IValidator<CreateLocationCommand> _validator;

    public CreateLocationCommandHandler(
        ILocationRepository locationRepository,
        IDateTimeProvider date,
        ITransactionManager transactionManager,
        ILogger<CreateLocationCommandHandler> logger,
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

        var transactionScopeResult = await _transactionManager.BeginTransactionAsync(cancellationToken);
        if (transactionScopeResult.IsFailure)
            return transactionScopeResult.Error;

        using var transactionScope = transactionScopeResult.Value;
        
        var address = Address.Create(command.Request.Address.Country,
            command.Request.Address.City,
            command.Request.Address.Street,
            command.Request.Address.Building,
            command.Request.Address.Office,
            command.Request.Address.PostalCode);

        if (address.IsFailure)
        {
            transactionScope.Rollback();
            return address.Error;
        }

        var locationResult = Location.Create(
            Guid.NewGuid(),
            command.Request.Name,
            address.Value,
            command.Request.Timezone,
            _date.UtcNow,
            isDeleted: false);
        
        if (locationResult.IsFailure)
        {
            transactionScope.Rollback();
            return locationResult.Error;
        }

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

        var commitResult = transactionScope.Commit();
        if (commitResult.IsFailure)
            return commitResult.Error;

        _logger.LogInformation("Локация {Name} создана", locationResult.Value.Name.Value);
        return new CreateLocationResponse(locationResult.Value.Id);
    }
}
