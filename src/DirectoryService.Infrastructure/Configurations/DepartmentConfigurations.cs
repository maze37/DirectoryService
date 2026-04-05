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
            nameBuilder.Property<string>("Value")
                .HasColumnName("name")
                .IsRequired()
                .HasMaxLength(LenghtConstants.MAXLENGHT);
        });
        
        builder.ComplexProperty(d => d.Identifier, idBuilder =>
        {
            idBuilder.Property<string>("Value")
                .HasColumnName("identifier")
                .IsRequired()
                .HasMaxLength(LenghtConstants.MAXLENGHT);
        });
        
        builder.ComplexProperty(d => d.Path, pathBuilder =>
        {
            pathBuilder.Property<string>("Value")
                .HasColumnName("path")
                .IsRequired()
                .HasMaxLength(LenghtConstants.MAXLENGHT);
        });
        
        builder.ComplexProperty(d => d.Depth, depthBuilder =>
        {
            depthBuilder.Property<short>("Value")
                .HasColumnName("depth")
                .IsRequired();
        });
        
        builder.Property(d => d.ParentId)
            .HasColumnName("parent_id")
            .IsRequired(false);
        
        builder.HasOne<Department>()
            .WithMany(d => d.Children)
            .HasForeignKey(d => d.ParentId)
            .HasConstraintName("fk_departments_parent")
            .OnDelete(DeleteBehavior.Restrict);
        
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
            .HasForeignKey("DepartmentId")
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasMany(d => d.Positions)
            .WithOne()
            .HasForeignKey("DepartmentId")
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
    }
}