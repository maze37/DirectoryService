using DirectoryService.Domain;
using DirectoryService.Domain.Location;
using DirectoryService.Domain.Location.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DirectoryService.Infrastructure.Configurations;

public class LocationConfigurations : IEntityTypeConfiguration<Location>
{
    public void Configure(EntityTypeBuilder<Location> builder)
    {
        builder.ToTable("locations");
        builder.HasKey(l => l.Id);
        builder.Property(l => l.Id).HasColumnName("id");
        
        builder.Property(l => l.Name)
            .HasConversion(
                v => v.Value,
                v => LocationName.From(v))
            .HasColumnName("name")
            .IsRequired()
            .HasMaxLength(LocationName.MAX_NAME_LENGHT);
        
        builder.OwnsOne(l => l.Address, addrBuilder =>
        {
            addrBuilder.Property(a => a.Country)
                .HasColumnName("address_country")
                .IsRequired()
                .HasMaxLength(LenghtConstants.MAXLENGHT);
            addrBuilder.Property(a => a.City)
                .HasColumnName("address_city")
                .IsRequired()
                .HasMaxLength(LenghtConstants.MAXLENGHT);
            addrBuilder.Property(a => a.Street)
                .HasColumnName("address_street")
                .IsRequired()
                .HasMaxLength(LenghtConstants.MAXLENGHT);
            addrBuilder.Property(a => a.Building)
                .HasColumnName("address_building")
                .IsRequired()
                .HasMaxLength(LenghtConstants.MAXLENGHT);
            addrBuilder.Property(a => a.Office)
                .HasColumnName("address_office")
                .IsRequired(false)
                .HasMaxLength(LenghtConstants.MAXLENGHT);
            addrBuilder.Property(a => a.PostalCode)
                .HasColumnName("address_postal_code")
                .IsRequired(false)
                .HasMaxLength(LenghtConstants.MAXLENGHT);
        });
        
        builder.Property(l => l.Timezone)
            .HasConversion(
                v => v.Value,
                v => Timezone.From(v))
            .HasColumnName("timezone")
            .IsRequired()
            .HasMaxLength(LenghtConstants.MAXLENGHT);
        
        builder.Property(l => l.IsActive).HasColumnName("is_active").IsRequired();
        builder.Property(l => l.CreatedWhen).HasColumnName("created_when").IsRequired();
        builder.Property(l => l.UpdatedWhen).HasColumnName("updated_when").IsRequired();
        
        builder.HasMany(p => p.DepartmentLocations)
            .WithOne()
            .HasForeignKey(dl => dl.LocationId);
        
        builder.Property(p => p.Version)
            .HasColumnName("xmin")
            .HasColumnType("xid")
            .IsRowVersion();
    }
}
