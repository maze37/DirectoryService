namespace DirectoryService.Domain.DepartmentPositions;

public sealed class DepartmentPosition
{
    public Guid Id { get; private set; }
    public Guid DepartmentId { get; private set; }
    public Guid PositionId { get; private set; }
    
    private DepartmentPosition() { }

    public DepartmentPosition(Guid positionId, Guid departmentId)
    {
        DepartmentId = departmentId;
        PositionId = positionId;
    }
}