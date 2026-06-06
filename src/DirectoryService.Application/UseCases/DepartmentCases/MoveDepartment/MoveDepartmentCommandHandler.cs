using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Contracts.DepartmentContracts;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Shared.Core;

namespace DirectoryService.Application.UseCases.DepartmentCases.MoveDepartment;

public class MoveDepartmentCommandHandler : ICommandHandler<MoveDepartmentCommand, MoveDepartmentResponse>
{
    private readonly IDepartmentRepository _departmentRepository;
    private readonly ITransactionManager _transactionManager;
    private readonly IDateTimeProvider _dateTime;
    private readonly ILogger _logger;
    private readonly IValidator<MoveDepartmentCommand> _validator;

    public MoveDepartmentCommandHandler(
        IDepartmentRepository departmentRepository,
        ITransactionManager transactionManager,
        IDateTimeProvider dateTime,
        ILogger logger,
        IValidator<MoveDepartmentCommand> validator)
    {
        _departmentRepository = departmentRepository;
        _transactionManager = transactionManager;
        _dateTime = dateTime;
        _logger = logger;
        _validator = validator;
    }

    public async Task<Result<MoveDepartmentResponse, Error>> HandleAsync(
        MoveDepartmentCommand command,
        CancellationToken cancellationToken)
    {
        /*
        1) Проверить, что существует ли подразделение с таким departmentId и оно активно
        2) Проверить, что новый parentId (если не null) существует, активен и не совпадает с departmentId
        3) Нельзя выбрать родителем своё "дочернее" подразделение (чтобы не было зацикливания структуры)
        4) Изменить parentId у подразделения, пересчитать и обновить Path, Depth
        5) Для всех дочерних подразделений и их потомков обновить Path и Depth, использовать Ltree

        IsDescendantOrSelfAsync — проверка зацикливания
        MoveAsync — массовый UPDATE через Ltree
        MoveDepartment хендлер — вся логика
        */
        

    }
}