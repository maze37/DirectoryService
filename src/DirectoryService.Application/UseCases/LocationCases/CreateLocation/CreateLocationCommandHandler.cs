using DirectoryService.Application.Abstractions;
using DirectoryService.Contracts.LocationContracts;
using DirectoryService.Domain.Location;
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
        var location = Location.Create(
            Guid.NewGuid(),
            command.Name,
            command.Country,
            command.City,
            command.Street,
            command.Building,
            command.Office,
            command.PostalCode,
            command.Timezone,
            _date.UtcNow);

        if (location.IsFailure)
            return Result<CreateLocationResponse>.Failure(Error.Failure("Доменная ошибка от Location.Create()."));
        
        await _locationRepository.AddAsync(location.Value, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        
        var response = new CreateLocationResponse(location.Value.Id);

        return Result<CreateLocationResponse>.Success(response);
    }
}