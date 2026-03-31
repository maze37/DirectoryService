namespace DirectoryService.Domain;

public class DepartmentLocations
{
    public Guid DepartmentId { get; private set; }
    public Guid LocationId { get; private set; }
    
    private DepartmentLocations() { }

    public DepartmentLocations(Guid departmentId, Guid locationId)
    {
        DepartmentId = departmentId;
        LocationId = locationId;
    }
}