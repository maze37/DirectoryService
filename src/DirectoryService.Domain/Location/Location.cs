using CSharpFunctionalExtensions;
using DirectoryService.Domain.Location.ValueObjects;
using Shared.Base;

namespace DirectoryService.Domain.Location;

public class Location : AggregateRoot
{
    public LocationName Name { get; private set; }
    public Address Address { get; private set; }
    public Timezone Timezone { get; private set; }
    public bool IsActive { get; private set; }
    public DateTimeOffset CreatedWhen { get; private set; }
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
        LocationName name,
        Address address,
        Timezone timezone,
        DateTimeOffset createdWhen)
    {
        if (id == Guid.Empty)
            return Result.Failure<Location>("ID локации не может быть пустым.");
            
        return Result.Success(new Location(
            id,
            name,
            address,
            timezone,
            isActive: true,
            createdWhen));
    }
}