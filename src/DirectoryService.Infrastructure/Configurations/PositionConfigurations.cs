using DirectoryService.Domain;
using DirectoryService.Domain.Position;
using DirectoryService.Domain.Position.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DirectoryService.Infrastructure.Configurations;

public class PositionConfigurations : IEntityTypeConfiguration<Position>
{
    public void Configure(EntityTypeBuilder<Position> builder)
    {
        builder.ToTable("positions");
        
        builder.HasKey(p => p.Id);
        
        builder.Property(p => p.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();
        
        builder.Property(p => p.Name)
            .HasColumnName("name")
            .HasConversion(
                name => name.Value,
                value => PositionName.From(value))
            .IsRequired()
            .HasMaxLength(PositionName.MAX_NAME_LENGHT);
        
        builder.Property(p => p.Description)
            .HasColumnName("description")
            .IsRequired(false)
            .HasMaxLength(LenghtConstants.MAXLENGHT);
        
        builder.Property(p => p.IsActive)
            .HasColumnName("is_active")
            .IsRequired();
        
        builder.Property(p => p.CreatedWhen)
            .HasColumnName("created_when")
            .IsRequired();
        
        builder.Property(p => p.UpdatedWhen)
            .HasColumnName("updated_when")
            .IsRequired();

        builder.HasMany(p => p.DepartmentPosition)
            .WithOne()
            .HasForeignKey(dp => dp.PositionId);

        builder.Property(p => p.Version)
            .HasColumnName("xmin")
            .HasColumnType("xid")
            .IsRowVersion();
    }
}
