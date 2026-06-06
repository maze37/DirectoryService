using System.Data;
using CSharpFunctionalExtensions;

namespace DirectoryService.Application.Abstractions;

/// <summary>
/// UnitOfWork-Паттерн
/// </summary>
public interface ITransactionManager
{
    Task<UnitResult<Error>> SaveChangesAsync(CancellationToken cancellationToken);

    Task<Result<ITransactionScope, Error>> BeginTransactionAsync(
        CancellationToken cancellationToken = default,
        IsolationLevel? level = null);
}