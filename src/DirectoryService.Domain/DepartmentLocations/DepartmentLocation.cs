namespace DirectoryService.Domain.DepartmentLocations;

/// <summary>
/// Связь подразделения с локацией.
/// </summary>
public sealed class DepartmentLocation
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
    /// Идентификатор связанной локации.
    /// </summary>
    public Guid LocationId { get; private set; }
    /// <summary>
    /// Признак основной локации подразделения.
    /// </summary>
    public bool IsPrimary { get; private set; }
    
    /// <summary>
    /// Конструктор для восстановления объекта средствами EF Core.
    /// </summary>
    private DepartmentLocation() { }

    /// <summary>
    /// Инициализирует объект переданными значениями без дополнительных проверок.
    /// </summary>
    public DepartmentLocation(Guid departmentId, Guid locationId)
    {
        DepartmentId = departmentId;
        LocationId = locationId;
    }
}