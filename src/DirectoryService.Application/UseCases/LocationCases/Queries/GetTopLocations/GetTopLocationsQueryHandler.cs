using Core.Abstractions;
using Dapper;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Abstractions.Database;
using DirectoryService.Contracts.LocationContracts;

namespace DirectoryService.Application.UseCases.LocationCases.Queries.GetTopLocations;

public class GetTopLocationsQueryHandler : IQueryHandler<GetTopLocationsQuery, List<TopLocationDto>>
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly ILocationMediaEnrichmentService _locationMediaEnrichmentService;

    public GetTopLocationsQueryHandler(
        IDbConnectionFactory connectionFactory,
        ILocationMediaEnrichmentService locationMediaEnrichmentService)
    {
        _connectionFactory = connectionFactory;
        _locationMediaEnrichmentService = locationMediaEnrichmentService;
    }

    public async Task<List<TopLocationDto>?> HandleAsync(
        GetTopLocationsQuery query,
        CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);

        const string sql = """
                           SELECT
                               l.id,
                               l.name,
                               COUNT(dl.department_id) AS DepartmentCount,
                               l.photo_asset_id      AS PhotoAssetId,
                               l.address_city        AS City,
                               l.address_office      AS Office,
                               l.address_street      AS Street,
                               l.address_country     AS Country,
                               l.address_building    AS Building,
                               l.address_postal_code AS PostalCode
                           FROM locations l
                           LEFT JOIN department_locations dl ON l.id = dl.location_id
                           WHERE l.is_deleted = false
                           GROUP BY l.id, l.name, l.photo_asset_id,
                                    l.address_country, l.address_city,
                                    l.address_street, l.address_building,
                                    l.address_office, l.address_postal_code
                           ORDER BY DepartmentCount DESC, l.id
                           LIMIT 5;
                           """;

        var commandDefinition = new CommandDefinition(sql, cancellationToken: cancellationToken);

        var locations = await connection.QueryAsync<TopLocationDto, AddressDto, TopLocationDto>(
            commandDefinition,
            map: (location, address) => location with { Address = address },
            splitOn: "City");

        var locationsList = locations.ToList();
        
        var enrichTasks = locationsList.Select(async loc =>
        {
            var mediaAssetDto = await _locationMediaEnrichmentService.EnrichMediaAssetDtoAsync(loc.PhotoAssetId, cancellationToken);
            return loc with { MediaAssetDto = mediaAssetDto };
        });

        var enrichedLocations = await Task.WhenAll(enrichTasks);

        return enrichedLocations.ToList();
    }
}