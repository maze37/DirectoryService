using DirectoryService.Contracts.LocationContracts;
using Shared.Core;

namespace DirectoryService.Application.UseCases.LocationCases.Queries.GetTopLocations;

public record GetTopLocationsQuery : IQuery<List<TopLocationDto>>;