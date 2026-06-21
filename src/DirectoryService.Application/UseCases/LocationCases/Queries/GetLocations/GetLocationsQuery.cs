using DirectoryService.Contracts.Constants;
using DirectoryService.Contracts.LocationContracts;
using Shared.Core;

namespace DirectoryService.Application.UseCases.LocationCases.Queries.GetLocations;

public record GetLocationsQuery(GetLocationsRequest Request) : IQuery<PagedResult<LocationListItemDto>>;