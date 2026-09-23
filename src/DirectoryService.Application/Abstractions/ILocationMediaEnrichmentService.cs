using DirectoryService.Contracts.LocationContracts;

namespace DirectoryService.Application.Abstractions;

public interface ILocationMediaEnrichmentService
{
    Task<MediaAssetDto> EnrichMediaAssetDtoAsync(
        Guid? photoAssetId,
        CancellationToken cancellationToken);
}