using Dapper;
using DirectoryService.Application.Abstractions.Database;
using DirectoryService.Contracts.LocationContracts;
using Shared.Core;

namespace DirectoryService.Application.UseCases.LocationCases.Queries.GetTopLocations;

public class GetTopLocationsQueryHandler : IQueryHandler<GetTopLocationsQuery, List<TopLocationDto>>
{
    private readonly IDbConnectionFactory _connectionFactory;

    public GetTopLocationsQueryHandler(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
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
                               l.address_city        AS City,
                               l.address_office      AS Office,
                               l.address_street      AS Street,
                               l.address_country     AS Country,
                               l.address_building    AS Building,
                               l.address_postal_code AS PostalCode
                           FROM locations l 
                           LEFT JOIN department_locations dl ON l.id = dl.location_id
                           GROUP BY l.id, l.name, 
                                    l.address_country, l.address_city,
                                    l.address_street, l.address_building, 
                                    l.address_office, l.address_postal_code
                           ORDER BY DepartmentCount DESC, l.id
                           LIMIT 5;
                           """;

        var command = new CommandDefinition(
            sql,
            cancellationToken: cancellationToken);

        var result = await connection.QueryAsync<TopLocationDto, AddressDto, TopLocationDto>(
            command,
            map: (location, address) => location with { Address = address },
            splitOn: "City");

        return result.ToList();
    }
}