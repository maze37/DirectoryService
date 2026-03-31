using CSharpFunctionalExtensions;
using DirectoryService.Domain.Position.ValueObjects;
using Shared.Base;

namespace DirectoryService.Domain.Position;

/// <summary>
/// Должности сотрудников
/// </summary>
public class Position : AggregateRoot
{
    /// <summary>
    /// Название должности. Уникальное, от 3 до 100 символов.
    /// </summary>
    public PositionName Name { get; private set; }
    
    /// <summary>
    /// Описание должности. Необязательное поле, максимум 1000 символов.
    /// </summary>
    public Description? Description { get; private set; }
    
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
    
    private Position() : base(Guid.Empty) { }
    
    private Position(
        Guid id,
        PositionName name,
        Description? description,
        bool isActive,
        DateTimeOffset createdWhen) : base(id)
    {
        Name = name;
        Description = description;
        IsActive = isActive;
        CreatedWhen = createdWhen;
        UpdatedWhen = createdWhen;
    }
    
    public static Result<Position> Create(
        Guid id,
        string name,
        string? description,
        bool isActive,
        DateTimeOffset createdWhen)
    {
        if (id == Guid.Empty)
            return Result.Failure<Position>("ID позиции не может быть пустым.");
            
        var nameResult = PositionName.Create(name);
        if (nameResult.IsFailure)
            return Result.Failure<Position>(nameResult.Error);
            
        var descriptionResult = Description.Create(description);
        if (descriptionResult.IsFailure)
            return Result.Failure<Position>(descriptionResult.Error);
            
        return Result.Success(new Position(
            id,
            nameResult.Value,
            descriptionResult.Value,
            isActive,
            createdWhen));
    }
}