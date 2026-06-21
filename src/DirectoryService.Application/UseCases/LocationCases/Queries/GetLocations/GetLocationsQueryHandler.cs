using System.Data;
using Dapper;
using DirectoryService.Application.Abstractions.Database;
using DirectoryService.Contracts.Constants;
using DirectoryService.Contracts.LocationContracts;
using Microsoft.EntityFrameworkCore;
using Shared.Core;
using Shared.Exceptions;

namespace DirectoryService.Application.UseCases.LocationCases.Queries.GetLocations;

public class GetLocationsQueryHandler : IQueryHandler<GetLocationsQuery, PagedResult<LocationListItemDto>>
{
    private readonly IDbConnectionFactory _connectionFactory;

    public GetLocationsQueryHandler(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<PagedResult<LocationListItemDto>?> HandleAsync(
        GetLocationsQuery query,
        CancellationToken cancellationToken)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);

        var parameters = new DynamicParameters();
        var conditions = new List<string>();

        // Search
        if (!string.IsNullOrWhiteSpace(query.Request.Search)
            && query.Request.Search.Length <= LenghtConstants.MAXLENGHT)
        {
            conditions.Add("l.name ILIKE @search");
            parameters.Add("search", $"%{query.Request.Search}%");
        }

        // MinDepartmentCount
        if (query.Request.MinDepartmentCount.HasValue)
        {
            conditions.Add("tdc.total_count >= @minDepartmentCount");
            parameters.Add("minDepartmentCount", query.Request.MinDepartmentCount.Value);
        }

        var sortBy = query.Request.SortBy?.ToLower();
        var sortDir = query.Request.SortDir?.ToLower();

        if (!string.IsNullOrWhiteSpace(sortDir) && sortDir != "desc" && sortDir != "asc")
            throw new ValidationException(
                $"Недопустимое направление сортировки: sortDir={sortDir}. Допустимые значения: asc, desc.");

        var orderByClause = sortBy switch
        {
            "name" => sortDir == "desc" ? "l.name DESC" : "l.name ASC",
            "createdwhen" => sortDir == "desc" ? "l.created_when DESC" : "l.created_when ASC",
            "departmentcount" => sortDir == "desc" ? "tdc.total_count DESC" : "tdc.total_count ASC",
            null => "l.name ASC",
            _ => throw new ValidationException($"Недопустимое поле сортировки: sortBy={sortBy}")
        };
        
        // Валидация пагинации
        if (query.Request.Pagination.Page < 1)
        {
            throw new ValidationException("Номер страницы должен быть больше 0");
        }
        
        if (query.Request.Pagination.PageSize <= 0)
        {
            throw new ValidationException("Размер страницы должен быть больше 0");
        }

        if (query.Request.Pagination.PageSize > 100)
        {
            throw new ValidationException("Размер страницы не должен превышать 100");
        }

        parameters.Add("offset", (query.Request.Pagination.Page - 1) * query.Request.Pagination.PageSize, DbType.Int32);
        parameters.Add("page_size", query.Request.Pagination.PageSize, DbType.Int32);

        var whereClause = conditions.Count > 0 ? "WHERE " + string.Join(" AND ", conditions) : "";

        var sql = $"""
                    WITH total_departments_count AS (
                        SELECT 
                            l.id as location_id,
                            COUNT(dl.location_id) as total_count
                        FROM locations l
                        LEFT JOIN department_locations dl ON l.id = dl.location_id
                        GROUP BY l.id
                    )
                    SELECT
                        l.name,
                        l.created_when,
                        tdc.total_count as department_count,
                        l.address_country as Country,
                        l.address_street as Street,
                        l.address_city as City,
                        l.address_office as Office,
                        l.address_building as Building,
                        l.address_postal_code as PostalCode,
                        COUNT(*) OVER() as total_rows
                    FROM locations l 
                    LEFT JOIN total_departments_count tdc ON l.id = tdc.location_id
                    {whereClause}
                    ORDER BY {orderByClause}
                    LIMIT @page_size OFFSET @offset
                    """;

        long? totalCount = null;

        var commandDefinition = new CommandDefinition(
            commandText: sql,
            parameters: parameters,
            cancellationToken: cancellationToken);

        var locations = await connection.QueryAsync<LocationListItemDto, AddressDto, long, LocationListItemDto>(
            commandDefinition,
            map: (location, address, totalRows) =>
            {
                totalCount ??= totalRows;
                return location with { Address = address };
            },
            splitOn: "country,total_rows");

        return new PagedResult<LocationListItemDto>(locations.ToList(), totalCount ?? 0, query.Request.Pagination);
    }
}