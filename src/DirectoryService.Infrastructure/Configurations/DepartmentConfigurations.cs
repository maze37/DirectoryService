using DirectoryService.Domain;
using DirectoryService.Domain.Department;
using DirectoryService.Domain.Department.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Path = DirectoryService.Domain.Department.ValueObjects.Path;

namespace DirectoryService.Infrastructure.Configurations;

public class DepartmentConfigurations : IEntityTypeConfiguration<Department>
{
    public void Configure(EntityTypeBuilder<Department> builder)
    {
        builder.ToTable("departments");
        builder.HasKey(d => d.Id);
        builder.Property(d => d.Id).HasColumnName("id");

        builder.Property(d => d.DepartmentName)
            .HasColumnName("name")
            .HasConversion(
                name => name.Value,
                value => DepartmentName.From(value))
            .IsRequired()
            .HasMaxLength(DepartmentName.MAX_NAME_LENGHT);

        builder.Property(d => d.Identifier)
            .HasColumnName("identifier")
            .HasConversion(
                identifier => identifier.Value,
                value => Identifier.From(value))
            .IsRequired()
            .HasMaxLength(Identifier.IDENTIFIER_MAX_LENGTH);
        
        builder.Property(d => d.Path)
            .HasColumnName("path")
            .HasConversion(
                path => path.Value,
                value => Path.From(value))
            .IsRequired()
            .HasMaxLength(LenghtConstants.MAXLENGHT);

        builder.Property(c => c.ChildrenCount).HasColumnName("children_count").IsRequired();
        builder.Property(d => d.Depth).HasColumnName("depth").IsRequired();
        builder.Property(d => d.ParentId).HasColumnName("parent_id").IsRequired(false);
        builder.Property(d => d.IsActive).HasColumnName("is_active").IsRequired();
        builder.Property(d => d.CreatedWhen).HasColumnName("created_when").IsRequired();
        builder.Property(d => d.UpdatedWhen).HasColumnName("updated_when").IsRequired();

        builder.HasMany(d => d.Locations)
            .WithOne()
            .HasForeignKey(n => n.DepartmentId);

        builder.HasMany(d => d.Positions)
            .WithOne()
            .HasForeignKey(n => n.DepartmentId);

        builder.Navigation(d => d.Locations)
            .HasField("_departmentLocations")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Navigation(d => d.Children)
            .HasField("_children")
            .UsePropertyAccessMode(PropertyAccessMode.Field);
        
        builder.Navigation(d => d.Positions)
            .HasField("_departmentPositions")
            .UsePropertyAccessMode(PropertyAccessMode.Field);
        
        builder.HasOne(d => d.Parent)
            .WithMany(d => d.Children)
            .HasForeignKey(d => d.ParentId)
            .IsRequired(false);
        
        builder.Property(d => d.Version)
            .IsRowVersion();
    }
}
