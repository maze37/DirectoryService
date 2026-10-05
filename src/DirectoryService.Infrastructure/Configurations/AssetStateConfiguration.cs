using DirectoryService.Application.ReadModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DirectoryService.Infrastructure.Configurations;

public sealed class AssetStateConfiguration : IEntityTypeConfiguration<AssetState>
{
    public void Configure(EntityTypeBuilder<AssetState> builder)
    {
        builder.ToTable("asset_states", "public", table =>
            table.HasCheckConstraint("ck_asset_states_status", "status IN ('Ready', 'Deleted')"));
        builder.HasKey(x => x.AssetId);
        builder.Property(x => x.AssetId).HasColumnName("asset_id").ValueGeneratedNever();
        builder.Property(x => x.EntityId).HasColumnName("entity_id").IsRequired();
        builder.Property(x => x.EntityType).HasColumnName("entity_type").HasColumnType("text").IsRequired();
        builder.Property(x => x.AssetType).HasColumnName("asset_type").HasColumnType("text").IsRequired();
        builder.Property(x => x.Status).HasColumnName("status").HasConversion<string>().HasColumnType("text").IsRequired();
        builder.Property(x => x.OccurredAt).HasColumnName("occurred_at").HasColumnType("timestamp with time zone").IsRequired();
    }
}
