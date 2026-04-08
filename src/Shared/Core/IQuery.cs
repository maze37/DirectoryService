namespace Shared.Core;

/// <summary>
/// Маркерный интерфейс для запросов
/// </summary>
public interface IQuery { }

/// <summary>
/// Интерфейс для запросов, возвращающих результат
/// </summary>
/// <typeparam name="TResponse">Тип возвращаемых данных</typeparam>
public interface IQuery<TResponse> : IQuery { }