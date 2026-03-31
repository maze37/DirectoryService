using CSharpFunctionalExtensions;
using DirectoryService.Domain.Department.ValueObjects;
using Shared.Base;

namespace DirectoryService.Domain.Department;

/// <summary>
/// Отдел в компании (отдел разработки, отдел продаж)
/// </summary>
public class Department : AggregateRoot
{
    private readonly List<DepartmentLocations> _locations = [];
    private readonly List<DepartmentPositions> _positions = [];
    private readonly List<Department> _children  = [];
    
    public IReadOnlyList<DepartmentLocations> Locations => _locations.AsReadOnly();
    public IReadOnlyList<DepartmentPositions> Positions => _positions.AsReadOnly();
    public IReadOnlyList<Department> Children => _children.AsReadOnly();
    public Department? Parent { get; private set; }
    
    /// <summary>
    /// Название отдела.
    /// </summary>
    public Name Name { get; private set; }
    
    /// <summary>
    /// 
    /// </summary>
    public Identifier Identifier { get; private set; }
    
    /// <summary>
    /// FK → Department.Id; null — корень.
    /// </summary>
    public Guid? ParentId { get; private set; }
    
    /// <summary>
    /// Путь 
    /// </summary>
    public PathVO Path { get; private set; }
    
    /// <summary>
    /// Гоубина подразделения
    /// </summary>
    public Depth Depth { get; private set; }
    
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

    private Department(
        Guid id,
        Name name,
        Identifier identifier,
        Guid? parentId,
        PathVO path, 
        Depth depth,
        bool isActive,
        DateTimeOffset createdWhen) :base(id)
    {
        Name = name;
        Identifier = identifier;
        ParentId = parentId;
        Path = path;
        Depth = depth;
        IsActive = isActive;
        CreatedWhen = createdWhen;
        UpdatedWhen = createdWhen;
    }

    public static Result<Department> Create(
        Guid id,
        string name,
        string identifier,
        Guid? parentId,
        string path,
        short depth,
        bool isActive,
        DateTimeOffset createdWhen)
    {
        if (id == Guid.Empty)
            return Result.Failure<Department>("ID отдела не может быть пустым.");
        
        var nameResult = Name.Create(name);
        if (nameResult.IsFailure)
            return Result.Failure<Department>(nameResult.Error);
            
        var identifierResult = Identifier.Create(identifier);
        if (identifierResult.IsFailure)
            return Result.Failure<Department>(identifierResult.Error);
            
        var pathResult = PathVO.Create(path);
        if (pathResult.IsFailure)
            return Result.Failure<Department>(pathResult.Error);
            
        var depthResult = Depth.Create(depth);
        if (depthResult.IsFailure)
            return Result.Failure<Department>(depthResult.Error);

        return Result.Success(new Department(
            id,
            nameResult.Value,
            identifierResult.Value,
            parentId,
            pathResult.Value,
            depthResult.Value,
            isActive,
            createdWhen));
    }
}