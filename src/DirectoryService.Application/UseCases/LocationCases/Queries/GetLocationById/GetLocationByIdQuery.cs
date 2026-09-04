using Core.Abstractions;
using DirectoryService.Contracts.LocationContracts;

namespace DirectoryService.Application.UseCases.LocationCases.Queries.GetLocationById;

public record GetLocationByIdQuery(Guid Id) : IQuery<GetLocationDto>;