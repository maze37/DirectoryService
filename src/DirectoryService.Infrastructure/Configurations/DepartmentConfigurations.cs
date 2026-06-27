using DirectoryService.Contracts.Constants;
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

        builder.Property(d => d.Slug)
            .HasColumnName("slug")
            .HasConversion(
                slug => slug.Value,
                value => Slug.From(value))
            .IsRequired();
        
        builder.HasIndex(d => d.Slug)
            .IsUnique()
            .HasDatabaseName("ux_departments_slug");
        
        builder.Property(d => d.Path)
            .HasColumnName("path")
            .HasColumnType("ltree")
            .HasConversion(
                path => path.Value,
                value => Path.From(value))
            .IsRequired()
            .HasMaxLength(LenghtConstants.MAXLENGHT);

        builder.HasIndex(pi => pi.Path)
            .HasMethod("gist")
            .HasDatabaseName("idx_departments_path");
        
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
            .HasField("_childrenDepartments")
            .UsePropertyAccessMode(PropertyAccessMode.Field);
        
        builder.Navigation(d => d.Positions)
            .HasField("_departmentPositions")
            .UsePropertyAccessMode(PropertyAccessMode.Field);
        
        builder.HasMany(d => d.Children)
            .WithOne(x => x.Parent)
            .IsRequired(false)
            .HasForeignKey(d => d.ParentId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.Property(d => d.Version)
            .IsRowVersion();
    }
}
