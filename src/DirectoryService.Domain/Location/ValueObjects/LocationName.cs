using CSharpFunctionalExtensions;
using SharedKernel;

namespace DirectoryService.Domain.Location.ValueObjects;

/// <summary>
/// Название локации с ограничениями длины.
/// </summary>
public class LocationName : ValueObject
{
    /// <summary>
    /// Минимальная длина названия в символах.
    /// </summary>
    public const int MIN_NAME_LENGHT = 3;
    /// <summary>
    /// Максимальная длина названия в символах.
    /// </summary>
    public const int MAX_NAME_LENGHT = 120;
    
    /// <summary>
    /// Проверенное название без пробелов по краям.
    /// </summary>
    public string Value { get; }
    
    /// <summary>
    /// Создаёт объект из готового значения без валидации; для пользовательского ввода используйте Create.
    /// </summary>
    public static LocationName From(string value) => new LocationName(value);
    
    /// <summary>
    /// Инициализирует объект переданными значениями без дополнительных проверок.
    /// </summary>
    private LocationName(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Удаляет пробелы по краям и проверяет обязательность и допустимую длину названия.
    /// </summary>
    public static Result<LocationName, Error> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return GeneralErrors.ValueIsRequired("location.name");

        value = value.Trim();

        if (value.Length < MIN_NAME_LENGHT)
            return GeneralErrors.ValueIsInvalid("location.name", $"Название локации не может быть меньше {MIN_NAME_LENGHT} символов.");

        if (value.Length > MAX_NAME_LENGHT)
            return GeneralErrors.ValueIsInvalid("location.name", $"Название локации не может быть больше {MAX_NAME_LENGHT} символов.");
        
        return new LocationName(value);
    }
    
    /// <summary>
    /// Возвращает компоненты, по которым сравниваются значения объекта.
    /// </summary>
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    /// <summary>
    /// Возвращает строковое представление при неявном преобразовании.
    /// </summary>
    public static implicit operator string(LocationName name) => name.Value;
}
