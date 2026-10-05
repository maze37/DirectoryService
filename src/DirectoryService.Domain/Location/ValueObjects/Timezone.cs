using CSharpFunctionalExtensions;
using SharedKernel;

namespace DirectoryService.Domain.Location.ValueObjects;

/// <summary>
/// Идентификатор часового пояса, проверяемый по часовым поясам операционной системы.
/// </summary>
public class Timezone : ValueObject
{
    /// <summary>
    /// Идентификатор часового пояса.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Создаёт объект из готового значения без валидации; для пользовательского ввода используйте Create.
    /// </summary>
    public static Timezone From(string value) => new Timezone(value);
    
    /// <summary>
    /// Инициализирует объект переданными значениями без дополнительных проверок.
    /// </summary>
    private Timezone(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Проверяет, что часовой пояс задан и распознаётся операционной системой.
    /// </summary>
    public static Result<Timezone, Error> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return GeneralErrors.ValueIsRequired("timezone");

        try
        {
            var timeZone = TimeZoneInfo.FindSystemTimeZoneById(value);
            return new Timezone(value);
        }
        catch (TimeZoneNotFoundException)
        {
            return GeneralErrors.ValueIsInvalid("timezone", "Часовой пояс не найден. Используйте IANA код, например: Europe/Moscow");
        }
        catch (InvalidTimeZoneException)
        {
            return GeneralErrors.ValueIsInvalid("timezone", "Часовой пояс имеет некорректный формат.");
        }
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
    public static implicit operator string(Timezone timezone) => timezone.Value;
}