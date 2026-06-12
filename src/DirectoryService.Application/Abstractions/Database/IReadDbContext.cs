using DirectoryService.Domain.Department;
using DirectoryService.Domain.Location;
using DirectoryService.Domain.Position;

namespace DirectoryService.Application.Abstractions.Database;

public interface IReadDbContext
{
    IQueryable<Department> DepartmentsRead { get; }
    IQueryable<Location> LocationsRead { get; }
    IQueryable<Position> PositionsRead { get; } 
}