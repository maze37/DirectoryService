namespace DirectoryService.Application.ReadModels;

public enum AssetStatus
{
    Ready,
    Deleted
}

public sealed class AssetState
{
    public Guid AssetId { get; private set; }
    public Guid EntityId { get; private set; }
    public string EntityType { get; private set; } = null!;
    public string AssetType { get; private set; } = null!;
    public AssetStatus Status { get; private set; }
    public DateTimeOffset OccurredAt { get; private set; }

    private AssetState() { }
}
