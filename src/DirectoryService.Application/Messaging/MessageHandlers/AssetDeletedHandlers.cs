using DirectoryService.Application.Abstractions;
using IntegrationEvents.Files.Events;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Application.Messaging.MessageHandlers;

public class AssetDeletedHandler
{
    public static async Task Handle(
        AssetDeleted message,
        IAssetStateRepository repository,
        ILogger<AssetDeletedHandler> logger,
        CancellationToken cancellationToken)
    {
        await repository.MarkDeletedAsync(
            message.AssetId,
            message.EntityId,
            message.EntityType,
            message.AssetType,
            message.OccurredAt,
            cancellationToken);

        logger.LogInformation(
            "Обработан AssetDeleted для {AssetId}",
            message.AssetId);
    }
}