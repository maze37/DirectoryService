using DirectoryService.Application.Abstractions;
using DirectoryService.Contracts.LocationContracts;
using DirectoryService.Domain.Location;
using DirectoryService.Domain.Location.ValueObjects;
using Shared.Core;
using Shared.Result;

namespace DirectoryService.Application.UseCases.LocationCases.CreateLocation;

public class CreateLocationCommandHandler : ICommandHandler<CreateLocationCommand, CreateLocationResponse>
{
    private readonly ILocationRepository _locationRepository;
    private readonly IDateTimeProvider _date;
    private readonly IUnitOfWork _unitOfWork;
    
    public CreateLocationCommandHandler(
        ILocationRepository locationRepository,
        IDateTimeProvider date, 
        IUnitOfWork unitOfWork)
    {
        _locationRepository = locationRepository ?? throw new ArgumentNullException(nameof(locationRepository));
        _date = date ?? throw new ArgumentNullException(nameof(date));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<Result<CreateLocationResponse>> HandleAsync(
        CreateLocationCommand command,
        CancellationToken cancellationToken)
    {
        var nameResult = LocationName.Create(command.Name);
        if (nameResult.IsFailure)
            return Result<CreateLocationResponse>.Failure(Error.Failure(nameResult.Error));
            
        var addressResult = Address.Create(
            command.Address.Country,
            command.Address.City, 
            command.Address.Street,
            command.Address.Building,
            command.Address.Office,
            command.Address.PostalCode);
        if (addressResult.IsFailure)
            return Result<CreateLocationResponse>.Failure(Error.Failure(addressResult.Error));
            
        var timezoneResult = Timezone.Create(command.Timezone);
        if (timezoneResult.IsFailure)
            return Result<CreateLocationResponse>.Failure(Error.Failure(timezoneResult.Error));
        
        var locationResult = Location.Create(
            Guid.NewGuid(),
            nameResult.Value,
            addressResult.Value,
            timezoneResult.Value,
            _date.UtcNow);

        if (locationResult.IsFailure)
            return Result<CreateLocationResponse>.Failure(
                Error.Failure(locationResult.Error));
        
        await _locationRepository.AddAsync(locationResult.Value, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        
        var response = new CreateLocationResponse(locationResult.Value.Id);
        
        return Result<CreateLocationResponse>.Success(response);
    }
}