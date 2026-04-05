using DirectoryService.Domain.Department;
using DirectoryService.Domain.Location;
using Microsoft.EntityFrameworkCore;

namespace DirectoryService.Infrastructure;

public class AppDbContext : DbContext
{
    private readonly string _connectionString;

    public AppDbContext(string connectionString)
    {
        _connectionString = connectionString;
    }
    
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql(_connectionString);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
    
    
    public DbSet<Department> Departments { get; set; }
    public DbSet<Location> Locations { get; set; }
    
    
}