using System.Text.RegularExpressions;
using CSharpFunctionalExtensions;
using SharedKernel;

namespace DirectoryService.Domain.Department.ValueObjects;

/// <summary>
/// Символьный идентификатор подразделения из латинских букв.
/// </summary>
public class Slug : ValueObject
{
    /// <summary>
    /// Символьный идентификатор подразделения.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Создаёт объект из готового значения без валидации; для пользовательского ввода используйте Create.
    /// </summary>
    public static Slug From(string value) => new Slug(value);
    
    /// <summary>
    /// Инициализирует объект переданными значениями без дополнительных проверок.
    /// </summary>
    private Slug(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Проверяет непустое значение из латинских букв, сохраняя исходный регистр.
    /// </summary>
    public static Result<Slug, Error> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return GeneralErrors.ValueIsRequired("department Slug");
        }

        if (!Regex.IsMatch(value, @"^[a-zA-Z]*$"))
        {
            return GeneralErrors.ValueIsInvalid("department Slug", "Slug must be in Latin characters");
        }

        return new Slug(value);
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
    public static implicit operator string(Slug slug) => slug.Value;
}