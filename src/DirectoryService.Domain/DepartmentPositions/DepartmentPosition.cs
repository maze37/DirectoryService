namespace DirectoryService.Domain.DepartmentPositions;

/// <summary>
/// Связь подразделения с должностью.
/// </summary>
public sealed class DepartmentPosition
{
    /// <summary>
    /// Идентификатор сущности.
    /// </summary>
    public Guid Id { get; private set; }
    /// <summary>
    /// Идентификатор связанного подразделения.
    /// </summary>
    public Guid DepartmentId { get; private set; }
    /// <summary>
    /// Идентификатор связанной должности.
    /// </summary>
    public Guid PositionId { get; private set; }
    
    /// <summary>
    /// Конструктор для восстановления объекта средствами EF Core.
    /// </summary>
    private DepartmentPosition() { }

    /// <summary>
    /// Инициализирует объект переданными значениями без дополнительных проверок.
    /// </summary>
    public DepartmentPosition(Guid positionId, Guid departmentId)
    {
        DepartmentId = departmentId;
        PositionId = positionId;
    }
}