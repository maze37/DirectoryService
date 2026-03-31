namespace DirectoryService.Domain;

public class DepartmentPositions
{
    public Guid DepartmentId { get; set; }
    public Guid PositionId { get; set; }
    
    private DepartmentPositions() { }

    public DepartmentPositions(Guid positionId, Guid departmentId)
    {
        DepartmentId = departmentId;
        PositionId = positionId;
    }
}