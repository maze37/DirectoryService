using DirectoryService.Application.ReadModels;

namespace DirectoryService.Application.Abstractions;

public interface IAssetStateRepository
{
    Task<AssetState?> GetByIdAsync(Guid assetId, CancellationToken cancellationToken);

    Task MarkReadyAsync(
        Guid assetId,
        Guid entityId,
        string entityType,
        string assetType,
        DateTimeOffset occurredAt,
        CancellationToken cancellationToken);

    Task MarkDeletedAsync(
        Guid assetId,
        Guid entityId,
        string entityType,
        string assetType,
        DateTimeOffset occurredAt,
        CancellationToken cancellationToken);
}