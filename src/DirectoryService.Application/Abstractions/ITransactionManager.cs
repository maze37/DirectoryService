using System.Data;
using CSharpFunctionalExtensions;

namespace DirectoryService.Application.Abstractions;

/// <summary>
/// Управление транзакциями и сохранением изменений (Unit of Work).
/// </summary>
public interface ITransactionManager
{
    /// <summary>
    /// Сохраняет все изменения из ChangeTracker в базу данных.
    /// </summary>
    Task<UnitResult<Error>> SaveChangesAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Открывает новую транзакцию с указанным уровнем изоляции.
    /// </summary>
    Task<Result<ITransactionScope, Error>> BeginTransactionAsync(
        CancellationToken cancellationToken = default,
        IsolationLevel? level = null);
}