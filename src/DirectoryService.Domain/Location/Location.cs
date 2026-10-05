using CSharpFunctionalExtensions;
using DirectoryService.Domain.DepartmentLocations;
using DirectoryService.Domain.Location.ValueObjects;
using SharedKernel;

namespace DirectoryService.Domain.Location;

/// <summary>
/// Где находятся подразделения
/// </summary>
public sealed class Location
{
    /// <summary>
    /// Идентификатор сущности.
    /// </summary>
    public Guid Id { get; private set; }
    
    /// <summary>
    /// Название локации.
    /// </summary>
    public LocationName Name { get; private set; } = null!;
    
    /// <summary>
    /// Идентификатор фотографии локации в FileService.
    /// </summary>
    public Guid? PhotoAssetId { get; private set; }
    
    /// <summary>
    /// Адрес локации.
    /// </summary>
    public Address Address { get; private set; } = null!;
    
    /// <summary>
    /// Временная зона в IANA формате.
    /// </summary>
    public Timezone Timezone { get; private set; } = null!;
    
    /// <summary>
    /// Активна ли локация (флаг).
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
    /// Для связи м-м
    /// </summary>
    public IReadOnlyList<DepartmentLocation> DepartmentLocations { get; private set; } = null!;
    
    /// <summary>
    /// Версия записи для контроля конкурентных изменений.
    /// </summary>
    public long Version { get; private set; }
    
    // EF Core
    /// <summary>
    /// Конструктор для восстановления объекта средствами EF Core.
    /// </summary>
    private Location() { }

    /// <summary>
    /// Инициализирует объект переданными значениями без дополнительных проверок.
    /// </summary>
    private Location(
        Guid id,
        LocationName name,
        Address address,
        Timezone timezone,
        DateTimeOffset createdWhen,
        bool isDeleted)
    {
        Id = id;
        Name = name;
        Address = address;
        Timezone = timezone;
        IsActive = true;
        CreatedWhen = createdWhen;
        UpdatedWhen = createdWhen;
        IsDeleted = isDeleted;
    }
    
    /// <summary>
    /// Проверяет ID, название, адрес и часовой пояс, затем создаёт локацию.
    /// </summary>
    public static Result<Location, Error> Create(
        Guid id,
        string name,
        Address address,
        string timezone,
        DateTimeOffset createdWhen,
        bool isDeleted = false)
    {
        if (id == Guid.Empty)
            return GeneralErrors.ValueIsInvalid("id","ID локации не может быть пустым.");
            
        var nameResult = LocationName.Create(name);
        if (nameResult.IsFailure)
            return nameResult.Error;
            
        var addressResult = Address.Create(address.Country, address.City, address.Street, address.Building, address.Office, address.PostalCode);
        if (addressResult.IsFailure)
            return addressResult.Error;
            
        var timezoneResult = Timezone.Create(timezone);
        if (timezoneResult.IsFailure)
            return timezoneResult.Error;
        
        return new Location(
            id, 
            nameResult.Value,
            addressResult.Value, 
            timezoneResult.Value,
            createdWhen,
            isDeleted);
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

    /// <summary>
    /// Записывает ID фотографии и обновляет время изменения; готовность ассета проверяет application-сценарий.
    /// </summary>
    public void AttachPhotoId(Guid photoAssetId)
    {
        PhotoAssetId = photoAssetId;
        UpdatedWhen = DateTimeOffset.UtcNow;
    }
    
    /// <summary>
    /// Заменяет ID фотографии и обновляет время изменения без обращения к FileService.
    /// </summary>
    public void UpdatePhotoId(Guid newPhotoAssetId)
    {
        PhotoAssetId = newPhotoAssetId;
        UpdatedWhen = DateTimeOffset.UtcNow;
    }
    
    /// <summary>
    /// Удаляет ссылку на фотографию и обновляет время изменения, не удаляя сам файл.
    /// </summary>
    public void RemovePhotoId()
    {
        PhotoAssetId = null;
        UpdatedWhen = DateTimeOffset.UtcNow;
    }
}