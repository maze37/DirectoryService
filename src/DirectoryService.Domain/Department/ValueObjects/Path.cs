using CSharpFunctionalExtensions;

namespace DirectoryService.Domain.Department.ValueObjects;

/// <summary>
/// Путь подразделения в иерархии, состоящий из разделённых точкой сегментов.
/// </summary>
public class Path : ValueObject
{
    /// <summary>
    /// Разделитель сегментов пути подразделения.
    /// </summary>
    public const char SEPARATOR = '.';
    
    /// <summary>
    /// Строковое представление пути подразделения.
    /// </summary>
    public string Value { get; }
    
    /// <summary>
    /// Создаёт объект из готового значения без валидации; для пользовательского ввода используйте Create.
    /// </summary>
    public static Path From(string value) => new Path(value);
    
    /// <summary>
    /// Инициализирует объект переданными значениями без дополнительных проверок.
    /// </summary>
    private Path(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Создаёт путь корневого подразделения из его slug.
    /// </summary>
    public static Path CreateParent(Slug slug)
    {
        return new Path(slug.Value);
    }
    
    /// <summary>
    /// Возвращает новый путь, добавляя slug дочернего подразделения к текущему пути.
    /// </summary>
    public Path CreateChild(Slug childSlug)
    {
        return new Path(Value + SEPARATOR + childSlug.Value);
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
    public static implicit operator string(Path path) => path.Value;
}