using Shared.Result; 

namespace Shared.Core;

/// <summary>
/// Обработчик запроса
/// </summary>
/// <typeparam name="TResponse">Тип ответа</typeparam>
/// <typeparam name="TQuery">Тип запроса</typeparam>
public interface IQueryHandler<in TQuery, TResponse> 
    where TQuery : IQuery<TResponse>
{
    Task<TResponse?> HandleAsync(TQuery query, CancellationToken cancellationToken = default);
}