using DirectoryService.Domain;
using DirectoryService.Domain.Department;
using DirectoryService.Domain.Position;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DirectoryService.Infrastructure.Configurations;

public class DepartmentPositionsConfigurations : IEntityTypeConfiguration<DepartmentPositions>
{
    public void Configure(EntityTypeBuilder<DepartmentPositions> builder)
    {
        builder.ToTable("department_positions");
        
        builder.HasKey(dp => new { dp.DepartmentId, dp.PositionId });
        
        builder.Property(dp => dp.DepartmentId)
            .HasColumnName("department_id")
            .IsRequired();
        
        builder.Property(dp => dp.PositionId)
            .HasColumnName("position_id")
            .IsRequired();
        
        builder.HasOne<Department>()
            .WithMany(d => d.Positions)
            .HasForeignKey(dp => dp.DepartmentId)
            .HasConstraintName("fk_department_positions_department")
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasOne<Position>()
            .WithMany()
            .HasForeignKey(dp => dp.PositionId)
            .HasConstraintName("fk_department_positions_position")
            .OnDelete(DeleteBehavior.Restrict);
    }
}