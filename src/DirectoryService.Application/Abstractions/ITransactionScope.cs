using CSharpFunctionalExtensions;

namespace DirectoryService.Application.Abstractions;

/// <summary>
/// Обёртка над активной транзакцией БД.
/// Dispose без Commit автоматически откатывает транзакцию.
/// </summary>
public interface ITransactionScope : IDisposable
{
    /// <summary>
    /// Фиксирует транзакцию.
    /// </summary>
    UnitResult<Error> Commit();

    /// <summary>
    /// Откатывает транзакцию.
    /// </summary>
    UnitResult<Error> Rollback();
}