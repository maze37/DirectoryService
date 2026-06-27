using CSharpFunctionalExtensions;
using DirectoryService.Domain.Department.ValueObjects;
using DirectoryService.Domain.DepartmentLocations;
using DirectoryService.Domain.DepartmentPositions;
using Shared.Base;
using Shared.Result;
using Path = DirectoryService.Domain.Department.ValueObjects.Path;

namespace DirectoryService.Domain.Department;

/// <summary>
/// Отдел в компании (отдел разработки, отдел продаж)
/// </summary>
public sealed class Department : AggregateRoot
{
    private readonly List<DepartmentLocation> _departmentLocations = [];
    private readonly List<DepartmentPosition> _departmentPositions = [];
    private readonly List<Department> _childrenDepartments  = [];
    
    public IReadOnlyList<DepartmentLocation> Locations => _departmentLocations.AsReadOnly();
    public IReadOnlyList<DepartmentPosition> Positions => _departmentPositions.AsReadOnly();
    public IReadOnlyList<Department> Children => _childrenDepartments.AsReadOnly();
    
    /// <summary>
    /// Название отдела.
    /// </summary>
    public DepartmentName DepartmentName { get; private set; } = null!;

    /// <summary>
    /// Идентификатор отдела.
    /// </summary>
    public Slug Slug { get; private set; } = null!;
    
    /// <summary>
    /// FK → Department.Id; null — корень.
    /// </summary>
    public Guid? ParentId { get; private set; }
    
    /// <summary>
    /// Путь 
    /// </summary>
    public Path Path { get; private set; } = null!;
    
    /// <summary>
    /// Гоубина подразделения
    /// </summary>
    public int Depth { get; private set; }

    /// <summary>
    /// Количество детей отдела
    /// </summary>
    public int ChildrenCount { get; private set; }
    
    /// <summary>
    /// Указатель на родителя
    /// </summary>
    public Department? Parent { get; private set; }
    
    /// <summary>
    /// Активен ли отдел. (Флаг)
    /// </summary>
    public bool IsActive { get; private set; }
    
    /// <summary>
    /// Дата и время создания.
    /// </summary>
    public DateTimeOffset CreatedWhen { get; private set; }
    
    /// <summary>
    /// Дата и время обновления.
    /// </summary>
    public DateTimeOffset UpdatedWhen { get; private set; }
    
    private Department() : base(Guid.Empty) { }

    public Department(
        Guid id,
        DepartmentName departmentName,
        Slug slug,
        Guid? parentId,
        Path path,
        int depth,
        Department? parent,
        DateTimeOffset createdWhen,
        List<Department> children,
        IEnumerable<DepartmentLocation> departmentLocations,
        IEnumerable<DepartmentPosition> departmentPositions) : base(id)
    {
        DepartmentName = departmentName;
        Slug = slug;
        ParentId = parentId;
        Path = path;
        Depth = depth;
        ChildrenCount = children.Count();
        IsActive = true;
        CreatedWhen = createdWhen;
        UpdatedWhen = createdWhen;
        Parent = parent;
        _childrenDepartments = children;
        _departmentLocations = departmentLocations.ToList();
        _departmentPositions = departmentPositions.ToList();
    }

    public static Result<Department, Error> CreateRoot(
        Guid id,
        string name,
        string identifier,
        int depth,
        DateTimeOffset createdWhen,
        List<DepartmentLocation> departmentLocations)
    {
        if (id == Guid.Empty)
            return GeneralErrors.ValueIsInvalid("id", "ID не может быть пустым");
        
        var nameResult = DepartmentName.Create(name);
        if (nameResult.IsFailure)
            return nameResult.Error;

        var identifierResult = Slug.Create(identifier);
        if (identifierResult.IsFailure)
            return identifierResult.Error;
        
        var path = Path.CreateParent(identifierResult.Value);

        return new Department(
            id,
            nameResult.Value,
            identifierResult.Value,
            parentId: null,
            path,
            depth: 0,
            parent: null,
            createdWhen,
            children: [],
            departmentLocations:  departmentLocations,
            departmentPositions: new List<DepartmentPosition>());
    }

    public static Result<Department, Error> CreateChild(
        Guid id,
        string name,
        string slug,
        Department parentDepartment,
        DateTimeOffset createdWhen,
        List<DepartmentLocation> departmentLocations)
    {
        if (id == Guid.Empty)
            return GeneralErrors.ValueIsInvalid("id", "ID не может быть пустым");

        var nameResult = DepartmentName.Create(name);
        if (nameResult.IsFailure)
            return nameResult.Error;

        var slugResult = Slug.Create(slug);
        if (slugResult.IsFailure)
            return slugResult.Error;
        
        var path = parentDepartment.Path.CreateChild(slugResult.Value);
        
        return new Department(
            id,
            nameResult.Value,
            slugResult.Value,
            parentId: parentDepartment.Id,
            path,
            depth: parentDepartment.Depth + 1,
            parent: parentDepartment,
            createdWhen,
            children: [],
            departmentLocations: departmentLocations,
            departmentPositions: new List<DepartmentPosition>());
    }

    public void UpdateLocations(List<DepartmentLocation> newLocations, DateTimeOffset updatedWhen)
    {
        _departmentLocations.Clear();
        _departmentLocations.AddRange(newLocations);
        UpdatedWhen = updatedWhen;
    }
    
    public void IncrementChildrenCount(DateTimeOffset updatedWhen)
    {
        ChildrenCount++;
        UpdatedWhen = updatedWhen;
    }

    public void DecrementChildrenCount(DateTimeOffset updatedWhen)
    {
        ChildrenCount--;
        UpdatedWhen = updatedWhen;
    }
}