using DirectoryService.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DirectoryService.Domain.Location;

namespace DirectoryService.Infrastructure.Configurations;

public class LocationConfigurations : IEntityTypeConfiguration<Location>
{
    public void Configure(EntityTypeBuilder<Location> builder)
    {
        builder.ToTable("locations");
        
        builder.HasKey(l => l.Id);
        
        builder.Property(l => l.Id).HasColumnName("id");
        
        builder.ComplexProperty(l => l.Name, nameBuilder =>
        {
            nameBuilder.Property<string>("Value")
                .HasColumnName("name")
                .IsRequired()
                .HasMaxLength(LenghtConstants.MAXLENGHT);
        });
        
        builder.ComplexProperty(l => l.Address, addrBuilder =>
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
        
        builder.ComplexProperty(l => l.Timezone, tzBuilder =>
        {
            tzBuilder.Property<string>("Value")
                .HasColumnName("timezone")
                .IsRequired()
                .HasMaxLength(LenghtConstants.MAXLENGHT);
        });
        
        builder.Property(l => l.IsActive)
            .HasColumnName("is_active")
            .IsRequired();
        
        builder.Property(l => l.CreatedWhen)
            .HasColumnName("created_when")
            .IsRequired();
        
        builder.Property(l => l.UpdatedWhen)
            .HasColumnName("updated_when")
            .IsRequired();
    }
} 