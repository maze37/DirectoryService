using DirectoryService.Domain.DepartmentPositions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DirectoryService.Infrastructure.Configurations;

public class DepartmentPositionsConfigurations : IEntityTypeConfiguration<DepartmentPosition>
{
    public void Configure(EntityTypeBuilder<DepartmentPosition> builder)
    {
        builder.ToTable("department_positions");

        builder.HasKey(dp => dp.Id)
            .HasName("pk_department_positions");

        builder.Property(dp => dp.Id)
            .IsRequired()
            .HasColumnName("id");

        builder.Property(dp => dp.DepartmentId)
            .IsRequired()
            .HasColumnName("department_id");

        builder.Property(dp => dp.PositionId)
            .IsRequired()
            .HasColumnName("position_id");
    }
}