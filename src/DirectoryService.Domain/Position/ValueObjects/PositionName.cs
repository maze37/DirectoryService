using CSharpFunctionalExtensions;
using SharedKernel;

namespace DirectoryService.Domain.Position.ValueObjects;

/// <summary>
/// Название должности с ограничениями длины.
/// </summary>
public class PositionName : ValueObject
{
    /// <summary>
    /// Минимальная длина названия в символах.
    /// </summary>
    public const int MIN_NAME_LENGTH = 3;
    /// <summary>
    /// Максимальная длина названия в символах.
    /// </summary>
    public const int MAX_NAME_LENGHT = 100;
    
    /// <summary>
    /// Проверенное название без пробелов по краям.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Создаёт объект из готового значения без валидации; для пользовательского ввода используйте Create.
    /// </summary>
    public static PositionName From(string value) => new PositionName(value);
    
    /// <summary>
    /// Инициализирует объект переданными значениями без дополнительных проверок.
    /// </summary>
    private PositionName(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Удаляет пробелы по краям и проверяет обязательность и допустимую длину названия.
    /// </summary>
    public static Result<PositionName, Error> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return GeneralErrors.ValueIsRequired("position.name");

        value = value.Trim();

        if (value.Length < MIN_NAME_LENGTH)
            return GeneralErrors.ValueIsInvalid("position.name", $"Название позиции не может быть меньше {MIN_NAME_LENGTH} символов.");

        if (value.Length > MAX_NAME_LENGHT)
            return GeneralErrors.ValueIsInvalid("position.name", $"Название позиции не может быть больше {MAX_NAME_LENGHT} символов.");

        return new PositionName(value);
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
    public static implicit operator string(PositionName name) => name.Value;
}
