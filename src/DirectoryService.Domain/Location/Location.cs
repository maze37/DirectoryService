using CSharpFunctionalExtensions;
using DirectoryService.Domain.Location.ValueObjects;
using Shared.Base;

namespace DirectoryService.Domain.Location;

/// <summary>
/// Где находятся подразделения
/// </summary>
public class Location : AggregateRoot
{
    /// <summary>
    /// Название локации. Уникальное, от 3 до 120 символов.
    /// Пример: "Главный офис Москва", "БЦ Москва-Сити"
    /// </summary>
    public LocationName Name { get; private set; }
    
    /// <summary>
    /// Физический адрес локации.
    /// Может быть строкой или структурированным объектом (страна, город, улица и т.д.)
    /// </summary>
    public Address Address { get; private set; }
    
    /// <summary>
    /// Часовой пояс локации в формате IANA.
    /// Пример: "Europe/Moscow", "Asia/Tokyo", "America/New_York"
    /// </summary>
    public Timezone Timezone { get; private set; }
    
    /// <summary>
    /// Флаг активности локации (soft delete).
    /// false - локация помечена как удаленная
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
    
    private Location() : base(Guid.Empty) { }
    
    private Location(
        Guid id,
        LocationName name,
        Address address,
        Timezone timezone,
        bool isActive,
        DateTimeOffset createdWhen) : base(id)
    {
        Name = name;
        Address = address;
        Timezone = timezone;
        IsActive = isActive;
        CreatedWhen = createdWhen;
        UpdatedWhen = createdWhen;
    }
    
    public static Result<Location> Create(
        Guid id,
        string name,
        string country,
        string city,
        string street,
        string building,
        string? office,
        string? postalCode,
        string timezone,
        DateTimeOffset createdWhen)
    {
        if (id == Guid.Empty)
            return Result.Failure<Location>("ID локации не может быть пустым.");
            
        var nameResult = LocationName.Create(name);
        if (nameResult.IsFailure)
            return Result.Failure<Location>(nameResult.Error);
            
        var addressResult = Address.Create(country, city, street, building, office, postalCode);
        if (addressResult.IsFailure)
            return Result.Failure<Location>(addressResult.Error);
            
        var timezoneResult = Timezone.Create(timezone);
        if (timezoneResult.IsFailure)
            return Result.Failure<Location>(timezoneResult.Error);
            
        return Result.Success(new Location(
            id,
            nameResult.Value,
            addressResult.Value,
            timezoneResult.Value,
            isActive: true,
            createdWhen));
    }
}