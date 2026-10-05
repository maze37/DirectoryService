using CSharpFunctionalExtensions;
using SharedKernel;

namespace DirectoryService.Domain.Department.ValueObjects;

/// <summary>
/// Название подразделения с ограничениями длины.
/// </summary>
public class DepartmentName : ValueObject
{
    /// <summary>
    /// Минимальная длина названия в символах.
    /// </summary>
    public const int MIN_NAME_LENGHT = 3;
    /// <summary>
    /// Максимальная длина названия в символах.
    /// </summary>
    public const int MAX_NAME_LENGHT = 150;
    
    /// <summary>
    /// Проверенное название без пробелов по краям.
    /// </summary>
    public string Value { get; }
    
    /// <summary>
    /// Создаёт объект из готового значения без валидации; для пользовательского ввода используйте Create.
    /// </summary>
    public static DepartmentName From(string value) => new DepartmentName(value);
    
    /// <summary>
    /// Инициализирует объект переданными значениями без дополнительных проверок.
    /// </summary>
    private DepartmentName(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Удаляет пробелы по краям и проверяет обязательность и допустимую длину названия.
    /// </summary>
    public static Result<DepartmentName, Error> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return GeneralErrors.ValueIsRequired("department.name");

        value = value.Trim();

        if (value.Length < MIN_NAME_LENGHT)
            return GeneralErrors.ValueIsInvalid("department.name", $"Название не может быть короче {MIN_NAME_LENGHT} символов");

        if (value.Length > MAX_NAME_LENGHT)
            return GeneralErrors.ValueIsInvalid("department.name", $"Название не может быть длиннее {MAX_NAME_LENGHT} символов");
        
        return new DepartmentName(value);
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
    public static implicit operator string(DepartmentName departmentName) => departmentName.Value;
}
