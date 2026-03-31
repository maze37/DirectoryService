using CSharpFunctionalExtensions;
using ValueObject = Shared.Base.ValueObject;

namespace DirectoryService.Domain.Location.ValueObjects;

public class Address : ValueObject
{
    public string Country { get; }
    public string City { get; }
    public string Street { get; }
    public string Building { get; }
    public string? Office { get; }
    public string? PostalCode { get; }
    
    public string FullAddress => $"{Country}, {City}, {Street} {Building}" + 
                                   (Office != null ? $", офис {Office}" : "") +
                                   (PostalCode != null ? $", {PostalCode}" : "");
    
    private Address(
        string country,
        string city,
        string street,
        string building,
        string? office,
        string? postalCode)
    {
        Country = country;
        City = city;
        Street = street;
        Building = building;
        Office = office;
        PostalCode = postalCode;
    }

    public static Result<Address> Create(
        string country,
        string city,
        string street,
        string building,
        string? office = null,
        string? postalCode = null)
    {
        if (string.IsNullOrWhiteSpace(country))
            return Result.Failure<Address>("Страна не может быть пустой.");
            
        if (string.IsNullOrWhiteSpace(city))
            return Result.Failure<Address>("Город не может быть пустым.");
            
        if (string.IsNullOrWhiteSpace(street))
            return Result.Failure<Address>("Улица не может быть пустой.");
            
        if (string.IsNullOrWhiteSpace(building))
            return Result.Failure<Address>("Номер здания не может быть пустым.");
        
        return Result.Success(new Address(
            country.Trim(),
            city.Trim(),
            street.Trim(),
            building.Trim(),
            office?.Trim(),
            postalCode?.Trim()));
    }
    
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Country;
        yield return City;
        yield return Street;
        yield return Building;
        yield return Office ?? string.Empty;
        yield return PostalCode ?? string.Empty;
    }

    public static implicit operator string(Address address) => address.FullAddress;
}