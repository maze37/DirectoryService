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
    /// <summary>
    /// Выполняет запрос
    /// </summary>
    /// <param name="query">Запрос</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>Результат с данными</returns>
    Task<Result<TResponse>> HandleAsync(TQuery query, CancellationToken cancellationToken = default);
}