using CSharpFunctionalExtensions;
using DirectoryService.Domain.DepartmentLocations;
using DirectoryService.Domain.Location.ValueObjects;
using Shared.Base;
using Shared.Result;

namespace DirectoryService.Domain.Location;

/// <summary>
/// Где находятся подразделения
/// </summary>
public sealed class Location : AggregateRoot
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
    
    /// <summary>
    /// Для связи м-м
    /// </summary>
    public IReadOnlyList<DepartmentLocation> DepartmentLocations { get; private set; }
    
    // EF Core
    private Location() : base(Guid.Empty) { }
    
    private Location(
        Guid id,
        LocationName name,
        Address address,
        Timezone timezone,
        DateTimeOffset createdWhen) : base(id)
    {
        Name = name;
        Address = address;
        Timezone = timezone;
        IsActive = true;
        CreatedWhen = createdWhen;
        UpdatedWhen = createdWhen;
    }
    
    public static Result<Location, Error> Create(
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
            return GeneralErrors.ValueIsInvalid("id","ID локации не может быть пустым.");
            
        var nameResult = LocationName.Create(name);
        if (nameResult.IsFailure)
            return nameResult.Error;
            
        var addressResult = Address.Create(country, city, street, building, office, postalCode);
        if (addressResult.IsFailure)
            return addressResult.Error;
            
        var timezoneResult = Timezone.Create(timezone);
        if (timezoneResult.IsFailure)
            return timezoneResult.Error;
        
        return new Location(
            id, 
            nameResult.Value,
            addressResult.Value, 
            timezoneResult.Value,
            createdWhen);
    }
}