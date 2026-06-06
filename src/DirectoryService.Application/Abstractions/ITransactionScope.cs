using CSharpFunctionalExtensions;

namespace DirectoryService.Application.Abstractions;

public interface ITransactionScope : IDisposable
{
    UnitResult<Error> Commit();
    UnitResult<Error> Rollback();
}