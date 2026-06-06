namespace Shared.Base;

/// <summary>
/// Абстракция корня агрегата.
/// </summary>
public abstract class AggregateRoot : Entity
{
    protected AggregateRoot(Guid id) : base(id) { }
    
    /// <summary>
    /// Версия записи в БД.
    /// </summary>
    public uint Version { get; private set; }
    
    /// <summary>
    /// Увелечение версии для записи.
    /// </summary>
    public void IncreaseVersion() =>  Version++;
}