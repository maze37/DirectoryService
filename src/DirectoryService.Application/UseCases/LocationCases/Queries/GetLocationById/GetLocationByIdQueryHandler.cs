using Core.Abstractions;
using DirectoryService.Application.Abstractions.Database;
using DirectoryService.Contracts.LocationContracts;
using Microsoft.EntityFrameworkCore;

namespace DirectoryService.Application.UseCases.LocationCases.Queries.GetLocationById;

public class GetLocationByIdQueryHandler : IQueryHandler<GetLocationByIdQuery, GetLocationDto>
{
    private readonly IReadDbContext _readDbContext;
    
    public GetLocationByIdQueryHandler(IReadDbContext readDbContext)
    {
        _readDbContext = readDbContext;
    }

    public async Task<GetLocationDto?> HandleAsync(
        GetLocationByIdQuery query,
        CancellationToken cancellationToken = default)
    {
        var location = await _readDbContext.LocationsRead
            .Where(l => l.Id == query.Id)
            .Select(l => new GetLocationDto
            {
                Id = l.Id,
                Name = l.Name,
                Country = l.Address.Country,
                City = l.Address.City,
                Street = l.Address.Street,
                Building = l.Address.Building,
                Office = l.Address.Office,
                PostalCode = l.Address.PostalCode,
                Timezone = l.Timezone,
                IsActive = l.IsActive,
                CreatedWhen = l.CreatedWhen,
                UpdatedWhen = l.UpdatedWhen
            })
            .FirstOrDefaultAsync(cancellationToken);

        return location;
    }
}