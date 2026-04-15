using CSharpFunctionalExtensions;
using DirectoryService.Contracts.LocationContracts;
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
    public LocationName Name { get; private set; } = null!;
    public Address Address { get; private set; } = null!;
    public Timezone Timezone { get; private set; } = null!;
    public bool IsActive { get; private set; }
    public DateTimeOffset CreatedWhen { get; private set; }
    public DateTimeOffset UpdatedWhen { get; private set; }
    
    /// <summary>
    /// Для связи м-м
    /// </summary>
    public IReadOnlyList<DepartmentLocation> DepartmentLocations { get; private set; } = null!;
    
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
        AddressDto address,
        string timezone,
        DateTimeOffset createdWhen)
    {
        if (id == Guid.Empty)
            return GeneralErrors.ValueIsInvalid("id","ID локации не может быть пустым.");
            
        var nameResult = LocationName.Create(name);
        if (nameResult.IsFailure)
            return nameResult.Error;
            
        var addressResult = Address.Create(address.Country, address.City, address.Street, address.Building, address.Office, address.PostalCode);
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