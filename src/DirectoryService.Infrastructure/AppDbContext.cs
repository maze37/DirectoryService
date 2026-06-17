using DirectoryService.Application.Abstractions.Database;
using DirectoryService.Domain.Department;
using DirectoryService.Domain.DepartmentLocations;
using DirectoryService.Domain.DepartmentPositions;
using DirectoryService.Domain.Location;
using DirectoryService.Domain.Position;
using Microsoft.EntityFrameworkCore;

namespace DirectoryService.Infrastructure;

public class AppDbContext : DbContext, IReadDbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    
    // Для изменения состояния.
    public DbSet<Department> Departments { get; set; }
    public DbSet<Location> Locations { get; set; }
    public DbSet<Position> Positions { get; set; }
    public DbSet<DepartmentLocation> DepartmentLocations { get; set; }
    public DbSet<DepartmentPosition> DepartmentPositions { get; set; }

    // Для чтения.
    public IQueryable<Department> DepartmentsRead => Set<Department>().AsQueryable().AsNoTracking();
    public IQueryable<Location> LocationsRead => Set<Location>().AsQueryable().AsNoTracking();
    public IQueryable<Position> PositionsRead => Set<Position>().AsQueryable().AsNoTracking();
    public IQueryable<DepartmentLocation> DepartmentLocationsRead => Set<DepartmentLocation>().AsQueryable().AsNoTracking();
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);
        
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasPostgresExtension("ltree");

        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(AppDbContext).Assembly);
    }
}