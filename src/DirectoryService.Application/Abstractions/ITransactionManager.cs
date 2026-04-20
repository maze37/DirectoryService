using CSharpFunctionalExtensions;
using Shared.Core;

namespace DirectoryService.Application.Abstractions;

/// <summary>
/// UnitOfWork Паттерн
/// </summary>
public interface ITransactionManager
{
    Task<Result<Unit, Error>> SaveChangesAsync(CancellationToken cancellationToken = default);
}