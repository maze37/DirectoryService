using DirectoryService.Domain;
using DirectoryService.Domain.Department;
using DirectoryService.Domain.Location;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DirectoryService.Infrastructure.Configurations;

public class DepartmentLocationsConfigurations : IEntityTypeConfiguration<DepartmentLocations>
{
    public void Configure(EntityTypeBuilder<DepartmentLocations> builder)
    {
        builder.ToTable("department_locations");
        
        builder.HasKey(dl => new { dl.DepartmentId, dl.LocationId });
        
        builder.Property(dl => dl.DepartmentId)
            .HasColumnName("department_id")
            .IsRequired();
        
        builder.Property(dl => dl.LocationId)
            .HasColumnName("location_id")
            .IsRequired();
        
        builder.HasOne<Department>()
            .WithMany(d => d.Locations)
            .HasForeignKey(dl => dl.DepartmentId)
            .HasConstraintName("fk_department_locations_department")
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasOne<Location>()
            .WithMany()
            .HasForeignKey(dl => dl.LocationId)
            .HasConstraintName("fk_department_locations_location")
            .OnDelete(DeleteBehavior.Restrict);
    }
}