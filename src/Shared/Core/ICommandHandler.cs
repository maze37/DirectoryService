namespace Shared.Core;

/// <summary>
/// Обработчик команды, возвращающей результат.
/// </summary>
/// <typeparam name="TCommand">Тип команды.</typeparam>
/// <typeparam name="TResponse">Тип возвращаемых данных.</typeparam>
public interface ICommandHandler<in TCommand, TResponse> 
    where TCommand : ICommand
{
    Task<CSharpFunctionalExtensions.Result<TResponse>> HandleAsync(TCommand command, CancellationToken cancellationToken = default);
}

/// <summary>
/// Обработчик команды без возвращаемых данных.
/// </summary>
/// <typeparam name="TCommand">Тип команды.</typeparam>
public interface ICommandHandler<in TCommand> 
    where TCommand : ICommand
{
    Task<CSharpFunctionalExtensions.Result> HandleAsync(TCommand command, CancellationToken cancellationToken = default);
}