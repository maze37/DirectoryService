using DirectoryService.Application.Abstractions;
using IntegrationEvents.Files.Events;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Application.Messaging.MessageHandlers;

public class AssetReadyHandler
{
    public static async Task Handle(
        AssetReady message,
        IAssetStateRepository repository,
        ILogger<AssetReadyHandler> logger,
        CancellationToken cancellationToken)
    {
        await repository.MarkReadyAsync(
            message.AssetId,
            message.EntityId,
            message.EntityType,
            message.AssetType,
            message.OccurredAt,
            cancellationToken);

        logger.LogInformation(
            "Обработан AssetReady для {AssetId}",
            message.AssetId);
    }
}