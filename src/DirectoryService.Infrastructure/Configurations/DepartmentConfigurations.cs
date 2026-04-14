using DirectoryService.Domain;
using DirectoryService.Domain.Department;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DirectoryService.Infrastructure.Configurations;

public class DepartmentConfigurations : IEntityTypeConfiguration<Department>
{
    public void Configure(EntityTypeBuilder<Department> builder)
    {
        builder.ToTable("departments");
        
        builder.HasKey(d => d.Id);
        
        builder.Property(d => d.Id).HasColumnName("id");
        
        builder.ComplexProperty(d => d.Name, nameBuilder =>
        {
            nameBuilder.Property<string>(n => n.Value)
                .HasColumnName("name")
                .IsRequired()
                .HasMaxLength(LenghtConstants.MAXLENGHT);
        });
        
        builder.ComplexProperty(d => d.Identifier, idBuilder =>
        {
            idBuilder.Property<string>(n => n.Value)
                .HasColumnName("identifier")
                .IsRequired()
                .HasMaxLength(LenghtConstants.MAXLENGHT);
        });
        
        builder.ComplexProperty(d => d.Path, pathBuilder =>
        {
            pathBuilder.Property<string>(n => n.Value)
                .HasColumnName("path")
                .IsRequired()
                .HasMaxLength(LenghtConstants.MAXLENGHT);
        });
        
        builder.Property(c => c.ChildrenCount)
            .HasColumnName("children_count")
            .IsRequired();
        
        builder.Property(d => d.Depth)
            .HasColumnName("depth")
            .IsRequired();
        
        builder.Property(d => d.ParentId)
            .HasColumnName("parent_id")
            .IsRequired(false);

        builder.HasOne<Department>()
            .WithMany(d => d.Children)
            .HasForeignKey(d => d.ParentId)
            .HasConstraintName("fk_departments_parent");
        
        builder.Property(d => d.IsActive)
            .HasColumnName("is_active")
            .IsRequired();
        
        builder.Property(d => d.CreatedWhen)
            .HasColumnName("created_when")
            .IsRequired();
        
        builder.Property(d => d.UpdatedWhen)
            .HasColumnName("updated_when")
            .IsRequired();

        builder.HasMany(d => d.Locations)
            .WithOne()
            .HasForeignKey(n => n.DepartmentId);

        builder.HasMany(d => d.Positions)
            .WithOne()
            .HasForeignKey(n => n.DepartmentId);
    }
}