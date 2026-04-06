using DirectoryService.Domain;
using DirectoryService.Domain.Position;
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
        
        builder.ComplexProperty(p => p.Name, nameBuilder =>
        {
            nameBuilder.Property<string>("Value")
                .HasColumnName("name")
                .IsRequired()
                .HasMaxLength(LenghtConstants.MAXLENGHT);
        });
        
        builder.Property(p => p.Description)
            .HasColumnName("description")
            .IsRequired(false)
            .HasMaxLength(1000);
        
        builder.Property(p => p.IsActive)
            .HasColumnName("is_active")
            .IsRequired();
        
        builder.Property(p => p.CreatedWhen)
            .HasColumnName("created_when")
            .IsRequired();
        
        builder.Property(p => p.UpdatedWhen)
            .HasColumnName("updated_when")
            .IsRequired();
    }
}