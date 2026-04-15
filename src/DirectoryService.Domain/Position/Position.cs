using CSharpFunctionalExtensions;
using DirectoryService.Domain.DepartmentPositions;
using DirectoryService.Domain.Position.ValueObjects;
using Shared.Base;
using Shared.Result;

namespace DirectoryService.Domain.Position;

/// <summary>
/// Должности сотрудников
/// </summary>
public sealed class Position : AggregateRoot
{
    /// <summary>
    /// Название должности. Уникальное, от 3 до 100 символов.
    /// </summary>
    public PositionName Name { get; private set; }
    
    /// <summary>
    /// Описание должности. Необязательное поле, максимум 1000 символов.
    /// </summary>
    public string? Description { get; private set; }
    
    /// <summary>
    /// Флаг активности должности (soft delete).
    /// false - должность помечена как удаленная
    /// </summary>
    public bool IsActive { get; private set; }
    
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
    public IReadOnlyList<DepartmentPosition> DepartmentPosition { get; private set; }
    
    // EF Core
    private Position() : base(Guid.Empty) { }
    
    private Position(
        Guid id,
        PositionName name,
        string? description,
        DateTimeOffset createdWhen) : base(id)
    {
        Name = name;
        Description = description;
        IsActive = true;
        CreatedWhen = createdWhen;
        UpdatedWhen = createdWhen;
    }
    
    public static Result<Position, Error> Create(
        Guid id,
        string name,
        string? description,
        DateTimeOffset createdWhen)
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
            createdWhen);
    }
}