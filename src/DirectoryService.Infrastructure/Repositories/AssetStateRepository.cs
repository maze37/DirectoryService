using DirectoryService.Application.ReadModels;
using DirectoryService.Application.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace DirectoryService.Infrastructure.Repositories;

public sealed class AssetStateRepository : IAssetStateRepository
{
    private readonly DirectoryServiceDbContext _context;

    public AssetStateRepository(DirectoryServiceDbContext context)
    {
        _context = context;
    }

    public Task<AssetState?> GetByIdAsync(Guid assetId, CancellationToken cancellationToken)
    {
        return _context.AssetStates
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.AssetId == assetId, cancellationToken);
    }

    public async Task MarkReadyAsync(
        Guid assetId,
        Guid entityId,
        string entityType,
        string assetType,
        DateTimeOffset occurredAt,
        CancellationToken cancellationToken)
    {
        await _context.Database.ExecuteSqlInterpolatedAsync(
            $"""
             INSERT INTO public.asset_states
                 (asset_id, entity_id, entity_type,
                  asset_type, status, occurred_at)
             VALUES
                 ({assetId}, {entityId}, {entityType},
                  {assetType}, 'Ready', {occurredAt})
             ON CONFLICT (asset_id) DO NOTHING
             """,
            cancellationToken);
    }

    public async Task MarkDeletedAsync(
        Guid assetId,
        Guid entityId,
        string entityType,
        string assetType,
        DateTimeOffset occurredAt,
        CancellationToken cancellationToken)
    {
        await _context.Database.ExecuteSqlInterpolatedAsync(
            $"""
             INSERT INTO public.asset_states AS current_state
                 (asset_id, entity_id, entity_type,
                  asset_type, status, occurred_at)
             VALUES
                 ({assetId}, {entityId}, {entityType},
                  {assetType}, 'Deleted', {occurredAt})
             ON CONFLICT (asset_id) DO UPDATE
             SET status = 'Deleted',
                 occurred_at = GREATEST(
                     current_state.occurred_at,
                     EXCLUDED.occurred_at)
             WHERE current_state.status <> 'Deleted'
             """,
            cancellationToken);
    }
}