using Core.Abstractions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Abstractions.Database;
using DirectoryService.Contracts.LocationContracts;
using Microsoft.EntityFrameworkCore;

namespace DirectoryService.Application.UseCases.LocationCases.Queries.GetLocationById;

public class GetLocationByIdQueryHandler : IQueryHandler<GetLocationByIdQuery, GetLocationDto>
{
    private readonly IReadDbContext _readDbContext;
    private readonly ILocationMediaEnrichmentService _locationMediaEnrichmentService;
    
    public GetLocationByIdQueryHandler(
        IReadDbContext readDbContext, 
        ILocationMediaEnrichmentService locationMediaEnrichmentService)
    {
        _readDbContext = readDbContext;
        _locationMediaEnrichmentService = locationMediaEnrichmentService;
    }

    public async Task<GetLocationDto?> HandleAsync(
        GetLocationByIdQuery query,
        CancellationToken cancellationToken = default)
    {
        var location = await _readDbContext.LocationsRead
            .Where(l => l.Id == query.Id)
            .Select(l => new
            {
                l.Id,
                l.Name,
                l.Address.Country,
                l.Address.City,
                l.Address.Street,
                l.Address.Building,
                l.Address.Office,
                l.Address.PostalCode,
                l.Timezone,
                l.IsActive,
                l.CreatedWhen,
                l.UpdatedWhen,
                l.PhotoAssetId
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (location is null)
            return null;
        
        var mediaAssetDto = await _locationMediaEnrichmentService.EnrichMediaAssetDtoAsync(location.PhotoAssetId, cancellationToken);

        return new GetLocationDto
        {
            Id = location.Id,
            Name = location.Name,
            Country = location.Country,
            City = location.City,
            Street = location.Street,
            Building = location.Building,
            Office = location.Office,
            PostalCode = location.PostalCode,
            Timezone = location.Timezone,
            IsActive = location.IsActive,
            CreatedWhen = location.CreatedWhen,
            UpdatedWhen = location.UpdatedWhen,
            MediaAssetDto = mediaAssetDto
        };
    }
}