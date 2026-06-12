using DirectoryService.Contracts.LocationContracts;
using Shared.Core;

namespace DirectoryService.Application.UseCases.LocationCases.Queries.GetLocationById;

public record GetLocationByIdQuery(Guid Id) : IQuery<GetLocationDto>;