using CSharpFunctionalExtensions;
using DirectoryService.Contracts.Constants;
using DirectoryService.Domain.DepartmentPositions;
using DirectoryService.Domain.Position.ValueObjects;
using SharedKernel;

namespace DirectoryService.Domain.Position;

/// <summary>
/// Должности сотрудников
/// </summary>
public sealed class Position
{
    /// <summary>
    /// Идентификатор сущности.
    /// </summary>
    public Guid Id { get; private set; }
    
    /// <summary>
    /// Название должности. Уникальное, от 3 до 100 символов.
    /// </summary>
    public PositionName Name { get; private set; } = null!;
    
    /// <summary>
    /// Описание должности. Необязательное поле, максимум 1000 символов.
    /// </summary>
    public string? Description { get; private set; }
    
    /// <summary>
    /// Флаг активности должности.
    /// </summary>
    public bool IsActive { get; private set; }
    
    /// <summary>
    /// Soft Delete.
    /// </summary>
    public bool? IsDeleted { get; private set; }
    
    /// <summary>
    /// Дата и время удаления.
    /// </summary>
    public DateTimeOffset? DeletedWhen { get; private set; }
    
    /// <summary>
    /// Дата и время создания записи в UTC.
    /// </summary>
    public DateTimeOffset CreatedWhen { get; private set; }
    
    /// <summary>
    /// Дата и время последнего обновления записи в UTC.
    /// </summary>
    public DateTimeOffset UpdatedWhen { get; private set; }
    
    /// <summary>
    /// Для связи м-м
    /// </summary>
    public IReadOnlyList<DepartmentPosition> DepartmentPosition { get; private set; } = null!;
    
    /// <summary>
    /// Версия записи для контроля конкурентных изменений.
    /// </summary>
    public long Version { get; private set; }
    
    // EF Core
    /// <summary>
    /// Конструктор для восстановления объекта средствами EF Core.
    /// </summary>
    private Position() { }

    /// <summary>
    /// Инициализирует объект переданными значениями без дополнительных проверок.
    /// </summary>
    private Position(
        Guid id,
        PositionName name,
        string? description,
        DateTimeOffset createdWhen,
        List<DepartmentPosition> departmentPositions)
    {
        Id = id;
        Name = name;
        Description = description;
        IsActive = true;
        CreatedWhen = createdWhen;
        UpdatedWhen = createdWhen;
        DepartmentPosition = departmentPositions;
    }
    
    /// <summary>
    /// Проверяет ID, название и длину описания, затем создаёт должность с переданными связями.
    /// </summary>
    public static Result<Position, Error> Create(
        Guid id,
        string name,
        string? description,
        DateTimeOffset createdWhen,
        List<DepartmentPosition> departmentPositions)
    {
        if (id == Guid.Empty)
            return GeneralErrors.ValueIsInvalid("id","ID позиции не может быть пустым.");
            
        var nameResult = PositionName.Create(name);
        if (nameResult.IsFailure)
            return nameResult.Error;
            
        var desc = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        if (desc?.Length > LenghtConstants.MAXLENGHT)
            return GeneralErrors.ValueIsInvalid("desc", "Описание не может быть больше 1000 символов.");

        return new Position(
            id,
            nameResult.Value,
            desc,
            createdWhen,
            departmentPositions);
    }
    
    /// <summary>
    /// Принимает готовый VO + меняет имя должности.
    /// </summary>
    public void Rename(PositionName newName, DateTimeOffset dateTime)
    {
        Name = newName;
        UpdatedWhen = dateTime;
    }

    /// <summary>
    /// Помечает сущность удалённой и сохраняет время удаления, не удаляя запись физически.
    /// </summary>
    public void SoftDelete(DateTimeOffset deletedWhen)
    {
        IsDeleted = true;
        DeletedWhen = deletedWhen;
    }
    
    /// <summary>
    /// Снимает признак мягкого удаления и очищает время удаления.
    /// </summary>
    public void Restore()
    {
        IsDeleted = false;
        DeletedWhen = null;
    }
    
    /// <summary>
    /// Задаёт время удаления для тестовых сценариев фоновой очистки.
    /// </summary>
    public void SetDeletedWhenForTest(DateTimeOffset value) => DeletedWhen = value;
}