using CSharpFunctionalExtensions;
using SharedKernel;

namespace DirectoryService.Domain.Location.ValueObjects;

/// <summary>
/// Адрес локации с обязательными страной, городом, улицей и номером здания.
/// </summary>
public class Address : ValueObject
{
    /// <summary>
    /// Страна расположения локации.
    /// </summary>
    public string Country { get; }
    /// <summary>
    /// Город расположения локации.
    /// </summary>
    public string City { get; }
    /// <summary>
    /// Улица расположения локации.
    /// </summary>
    public string Street { get; }
    /// <summary>
    /// Номер или обозначение здания.
    /// </summary>
    public string Building { get; }
    /// <summary>
    /// Номер офиса, если указан.
    /// </summary>
    public string? Office { get; }
    /// <summary>
    /// Почтовый индекс, если указан.
    /// </summary>
    public string? PostalCode { get; }
    
    /// <summary>
    /// Полный адрес с необязательными офисом и почтовым индексом.
    /// </summary>
    public string FullAddress => $"{Country}, {City}, {Street} {Building}" + 
                                   (Office != null ? $", офис {Office}" : "") +
                                   (PostalCode != null ? $", {PostalCode}" : "");
    
    /// <summary>
    /// Инициализирует объект переданными значениями без дополнительных проверок.
    /// </summary>
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

    /// <summary>
    /// Проверяет обязательные части адреса и удаляет пробелы по краям переданных значений.
    /// </summary>
    public static Result<Address, Error> Create(
        string country,
        string city,
        string street,
        string building,
        string? office = null,
        string? postalCode = null)
    {
        if (string.IsNullOrWhiteSpace(country))
            return GeneralErrors.ValueIsRequired(nameof(country));
            
        if (string.IsNullOrWhiteSpace(city))
            return GeneralErrors.ValueIsRequired(nameof(city));
            
        if (string.IsNullOrWhiteSpace(street))
            return GeneralErrors.ValueIsRequired(nameof(street));
            
        if (string.IsNullOrWhiteSpace(building))
            return GeneralErrors.ValueIsRequired(nameof(building));
        
        return new Address(
            country.Trim(),
            city.Trim(),
            street.Trim(),
            building.Trim(),
            office?.Trim(),
            postalCode?.Trim());
    }
    
    /// <summary>
    /// Возвращает компоненты, по которым сравниваются значения объекта.
    /// </summary>
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Country;
        yield return City;
        yield return Street;
        yield return Building;
        yield return Office ?? string.Empty;
        yield return PostalCode ?? string.Empty;
    }

    /// <summary>
    /// Возвращает строковое представление при неявном преобразовании.
    /// </summary>
    public static implicit operator string(Address address) => address.FullAddress;
}