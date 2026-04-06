using DirectoryService.Domain;
using DirectoryService.Domain.Department;
using DirectoryService.Domain.Location;
using DirectoryService.Domain.Position;
using Microsoft.EntityFrameworkCore;

namespace DirectoryService.Infrastructure;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    
    public DbSet<Department> Departments { get; set; }
    public DbSet<Location> Locations { get; set; }
    public DbSet<Position> Positions { get; set; }
    public DbSet<DepartmentLocations> DepartmentLocations { get; set; }
    public DbSet<DepartmentPositions> DepartmentPositions { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);
        
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(AppDbContext).Assembly);
    }
}