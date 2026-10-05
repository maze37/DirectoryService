using CSharpFunctionalExtensions;
using DirectoryService.Domain.Department.ValueObjects;
using DirectoryService.Domain.DepartmentLocations;
using DirectoryService.Domain.DepartmentPositions;
using SharedKernel;
using Path = DirectoryService.Domain.Department.ValueObjects.Path;

namespace DirectoryService.Domain.Department;

/// <summary>
/// Отдел в компании (отдел разработки, отдел продаж)
/// </summary>
public sealed class Department
{
    /// <summary>
    /// Изменяемые связи подразделения с локациями.
    /// </summary>
    private readonly List<DepartmentLocation> _departmentLocations = [];
    /// <summary>
    /// Изменяемые связи подразделения с должностями.
    /// </summary>
    private readonly List<DepartmentPosition> _departmentPositions = [];
    /// <summary>
    /// Загруженные дочерние подразделения.
    /// </summary>
    private readonly List<Department> _childrenDepartments  = [];
    
    /// <summary>
    /// Связи подразделения с локациями, доступные только для чтения.
    /// </summary>
    public IReadOnlyList<DepartmentLocation> Locations => _departmentLocations.AsReadOnly();
    /// <summary>
    /// Связи подразделения с должностями, доступные только для чтения.
    /// </summary>
    public IReadOnlyList<DepartmentPosition> Positions => _departmentPositions.AsReadOnly();
    /// <summary>
    /// Загруженные дочерние подразделения, доступные только для чтения.
    /// </summary>
    public IReadOnlyList<Department> Children => _childrenDepartments.AsReadOnly();
    
    /// <summary>
    /// Идентификатор сущности.
    /// </summary>
    public Guid Id { get; private set; }
    
    /// <summary>
    /// Название отдела.
    /// </summary>
    public DepartmentName DepartmentName { get; private set; } = null!;

    /// <summary>
    /// Идентификатор отдела.
    /// </summary>
    public Slug Slug { get; private set; } = null!;
    
    /// <summary>
    /// FK - Department.Id; null — корень.
    /// </summary>
    public Guid? ParentId { get; private set; }
    
    /// <summary>
    /// Путь подразделения в иерархии от корня.
    /// </summary>
    public Path Path { get; private set; } = null!;
    
    /// <summary>
    /// Глубина подразделения в иерархии; у корня равна нулю.
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
    /// Soft Delete.
    /// </summary>
    public bool IsDeleted { get; private set; }
    
    /// <summary>
    /// Дата и время удаления.
    /// </summary>
    public DateTimeOffset? DeletedWhen { get; private set; }
    
    /// <summary>
    /// Дата и время создания.
    /// </summary>
    public DateTimeOffset CreatedWhen { get; private set; }
    
    /// <summary>
    /// Дата и время обновления.
    /// </summary>
    public DateTimeOffset UpdatedWhen { get; private set; }
    
    /// <summary>
    /// Версия записи для контроля конкурентных изменений.
    /// </summary>
    public long Version { get; private set; }

    /// <summary>
    /// Конструктор для восстановления объекта средствами EF Core.
    /// </summary>
    private Department() { }

    /// <summary>
    /// Инициализирует объект переданными значениями без дополнительных проверок.
    /// </summary>
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
        IEnumerable<DepartmentPosition> departmentPositions)
    {
        Id = id;
        DepartmentName = departmentName;
        Slug = slug;
        ParentId = parentId;
        Path = path;
        Depth = depth;
        ChildrenCount = children.Count;
        IsActive = true;
        CreatedWhen = createdWhen;
        UpdatedWhen = createdWhen;
        Parent = parent;
        _childrenDepartments = children;
        _departmentLocations = departmentLocations.ToList();
        _departmentPositions = departmentPositions.ToList();
    }

    /// <summary>
    /// Создаёт корневое подразделение без родителя с глубиной ноль после проверки ID, имени и slug.
    /// </summary>
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

    /// <summary>
    /// Создаёт дочернее подразделение с путём и глубиной, вычисленными относительно родителя.
    /// </summary>
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

    /// <summary>
    /// Заменяет связи с локациями и обновляет время изменения подразделения.
    /// </summary>
    public void UpdateLocations(List<DepartmentLocation> newLocations, DateTimeOffset updatedWhen)
    {
        _departmentLocations.Clear();
        _departmentLocations.AddRange(newLocations);
        UpdatedWhen = updatedWhen;
    }
    
    /// <summary>
    /// Увеличивает число дочерних подразделений и обновляет время изменения.
    /// </summary>
    public void IncrementChildrenCount(DateTimeOffset updatedWhen)
    {
        ChildrenCount++;
        UpdatedWhen = updatedWhen;
    }

    /// <summary>
    /// Уменьшает число дочерних подразделений без проверки нижней границы и обновляет время изменения.
    /// </summary>
    public void DecrementChildrenCount(DateTimeOffset updatedWhen)
    {
        ChildrenCount--;
        UpdatedWhen = updatedWhen;
    }

    /// <summary>
    /// Помечает сущность удалённой и сохраняет время удаления, не удаляя запись физически.
    /// </summary>
    public void SoftDelete(DateTimeOffset deletedWhen)
    {
        IsDeleted = true;
        DeletedWhen = deletedWhen;
    }
    
    /// <summary>
    /// Снимает признак мягкого удаления и очищает время удаления.
    /// </summary>
    public void Restore()
    {
        IsDeleted = false;
        DeletedWhen = null;
    }
    
    /// <summary>
    /// Задаёт время удаления для тестовых сценариев фоновой очистки.
    /// </summary>
    public void SetDeletedWhenForTest(DateTimeOffset value) => DeletedWhen = value;
}