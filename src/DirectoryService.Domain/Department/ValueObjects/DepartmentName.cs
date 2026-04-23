using CSharpFunctionalExtensions;
using Shared.Result;
using ValueObject = Shared.Base.ValueObject;

namespace DirectoryService.Domain.Department.ValueObjects;

public class DepartmentName : ValueObject
{
    public const int MIN_NAME_LENGHT = 3;
    public const int MAX_NAME_LENGHT = 150;
    
    public string Value { get; }
    
    private DepartmentName(string value)
    {
        Value = value;
    }

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
    
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public static implicit operator string(DepartmentName departmentName) => departmentName.Value;
}
