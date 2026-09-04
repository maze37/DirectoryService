using Core.Abstractions;
using DirectoryService.Contracts.LocationContracts;

namespace DirectoryService.Application.UseCases.LocationCases.Queries.GetTopLocations;

public record GetTopLocationsQuery : IQuery<List<TopLocationDto>>;