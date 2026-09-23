using Core.Abstractions;
using Core.Database;
using Core.Validation;
using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Contracts.LocationContracts;
using DirectoryService.Domain.Location;
using DirectoryService.Domain.Location.ValueObjects;
using FluentValidation;
using Microsoft.Extensions.Logging;
using SharedKernel;
using IDateTimeProvider = DirectoryService.Application.Abstractions.IDateTimeProvider;

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
            return address.Error;

        var locationResult = Location.Create(
            Guid.NewGuid(),
            command.Request.Name,
            address.Value,
            command.Request.Timezone,
            _date.UtcNow,
            isDeleted: false);
        
        if (locationResult.IsFailure)
            return locationResult.Error;

        _locationRepository.Add(locationResult.Value);
        
        var saveResult = await _transactionManager.SaveChangesAsync(cancellationToken);
        if (saveResult.IsFailure)
            return saveResult.Error;

        var commitResult = transactionScope.Commit();
        if (commitResult.IsFailure)
            return commitResult.Error;

        _logger.LogInformation("Локация {Name} создана", locationResult.Value.Name.Value);
        return new CreateLocationResponse(locationResult.Value.Id);
    }
}
