using DirectoryService.Application.UseCases.DepartmentCases.Commands.AttachPositionToDepartment;
using DirectoryService.Application.UseCases.DepartmentCases.Commands.CreateDepartment;
using DirectoryService.Application.UseCases.DepartmentCases.Commands.DeleteDepartment;
using DirectoryService.Application.UseCases.DepartmentCases.Commands.DetachPositionFromDepartment;
using DirectoryService.Application.UseCases.DepartmentCases.Commands.MoveDepartment;
using DirectoryService.Application.UseCases.DepartmentCases.Commands.RestoreDepartment;
using DirectoryService.Application.UseCases.DepartmentCases.Commands.UpdateDepartmentLocations;
using DirectoryService.Application.UseCases.DepartmentCases.Queries.GetDepartmentById;
using DirectoryService.Application.UseCases.DepartmentCases.Queries.GetDepartments;
using DirectoryService.Application.UseCases.DepartmentCases.Queries.GetDepartmentsAncestors;
using DirectoryService.Application.UseCases.DepartmentCases.Queries.GetDepartmentsChildren;
using DirectoryService.Application.UseCases.DepartmentCases.Queries.GetDepartmentsTree;
using DirectoryService.Application.UseCases.DepartmentCases.Queries.GetDepartmentsTreeSearch;
using DirectoryService.Contracts.Constants;
using DirectoryService.Contracts.DepartmentContracts;
using DirectoryService.Presentation.ResponseExtensions;
using Microsoft.AspNetCore.Mvc;
using Shared.Core;
using Shared.Result;

namespace DirectoryService.Presentation.Controllers;

[ApiController]
[Route("api/departments")]
public class DepartmentController : ControllerBase
{
    private readonly ICommandHandler<CreateDepartmentCommand, CreateDepartmentResponse> _createHandler;
    private readonly ICommandHandler<UpdateDepartmentLocationsCommand, UpdateDepartmentLocationsResponse> _updateLocationsHandler;
    private readonly ICommandHandler<MoveDepartmentCommand, MoveDepartmentResponse> _moveHandler;
    private readonly ICommandHandler<DeleteDepartmentCommand, DeleteDepartmentResponse> _deleteHandler;
    private readonly ICommandHandler<AttachPositionToDepartmentCommand, AttachPositionToDepartmentResponse> _attachPositionHandler;
    private readonly ICommandHandler<DetachPositionFromDepartmentCommand, DetachPositionFromDepartmentResponse> _detachPositionHandler;
    private readonly ICommandHandler<RestoreDepartmentCommand, RestoreDepartmentResponse> _restoreHandler;
    private readonly IQueryHandler<GetDepartmentByIdQuery, GetDepartmentDto> _getByIdHandler;
    private readonly IQueryHandler<GetDepartmentsQuery, PagedResult<DepartmentListItemDto>> _getByFilterHandler;
    private readonly IQueryHandler<GetDepartmentsTreeQuery, IReadOnlyList<DepartmentTreeItemDto>> _getTreeHandler;
    private readonly IQueryHandler<GetDepartmentsChildrenQuery, IReadOnlyList<DepartmentTreeItemDto>?> _getChildrenHandler;
    private readonly IQueryHandler<GetDepartmentsAncestorsQuery, IReadOnlyList<DepartmentTreeItemDto>?> _getAncestorsHandler;
    private readonly IQueryHandler<GetDepartmentsTreeSearchQuery, IReadOnlyList<DepartmentTreeItemDto>> _getTreeSearchHandler;
    private readonly ILogger<DepartmentController> _logger;

    public DepartmentController(
        ICommandHandler<CreateDepartmentCommand, CreateDepartmentResponse> createHandler,
        ICommandHandler<UpdateDepartmentLocationsCommand, UpdateDepartmentLocationsResponse> updateLocationsHandler,
        ICommandHandler<MoveDepartmentCommand, MoveDepartmentResponse> moveHandler,
        ICommandHandler<DeleteDepartmentCommand, DeleteDepartmentResponse> deleteHandler,
        ICommandHandler<AttachPositionToDepartmentCommand, AttachPositionToDepartmentResponse> attachPositionHandler,
        ICommandHandler<DetachPositionFromDepartmentCommand, DetachPositionFromDepartmentResponse> detachPositionHandler,
        ICommandHandler<RestoreDepartmentCommand, RestoreDepartmentResponse> restoreHandler,
        IQueryHandler<GetDepartmentByIdQuery, GetDepartmentDto> getByIdHandler,
        IQueryHandler<GetDepartmentsQuery, PagedResult<DepartmentListItemDto>> getByFilterHandler,
        IQueryHandler<GetDepartmentsTreeQuery, IReadOnlyList<DepartmentTreeItemDto>> getTreeHandler,
        IQueryHandler<GetDepartmentsChildrenQuery, IReadOnlyList<DepartmentTreeItemDto>?> getChildrenHandler,
        IQueryHandler<GetDepartmentsAncestorsQuery, IReadOnlyList<DepartmentTreeItemDto>?> getAncestorsHandler,
        IQueryHandler<GetDepartmentsTreeSearchQuery, IReadOnlyList<DepartmentTreeItemDto>> getTreeSearchHandler,
        ILogger<DepartmentController> logger)
    {
        _createHandler = createHandler;
        _updateLocationsHandler = updateLocationsHandler;
        _moveHandler = moveHandler;
        _deleteHandler = deleteHandler;
        _attachPositionHandler = attachPositionHandler;
        _detachPositionHandler = detachPositionHandler;
        _restoreHandler = restoreHandler;
        _getByIdHandler = getByIdHandler;
        _getByFilterHandler = getByFilterHandler;
        _getTreeHandler = getTreeHandler;
        _getChildrenHandler = getChildrenHandler;
        _getAncestorsHandler = getAncestorsHandler;
        _getTreeSearchHandler = getTreeSearchHandler;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> CreateAsync(
        [FromBody] CreateDepartmentRequest request,
        CancellationToken cancellationToken = default)
    {
        var command = new CreateDepartmentCommand(request);

        var result = await _createHandler.HandleAsync(command, cancellationToken);

        if (result.IsFailure)
        {
            _logger.LogError("Ошибка создания отдела: {Error}", result.Error.ToResponse());
            return result.Error.ToResponse();
        }

        _logger.LogInformation("Отдел с ID: {DepartmentId} успешно создана", result.Value.Id);

        return Ok(Envelope.Ok(result.Value));
    }

    [HttpPut("{departmentId:guid}/locations")]
    public async Task<IActionResult> UpdateLocationsAsync(
        [FromRoute] Guid departmentId,
        [FromBody] UpdateDepartmentLocationsRequest request,
        CancellationToken cancellationToken = default)
    {
        var command = new UpdateDepartmentLocationsCommand(departmentId, request);

        var result = await _updateLocationsHandler.HandleAsync(command, cancellationToken);
        if (result.IsFailure)
        {
            _logger.LogError(
                "Ошибка обновления локаций подразделения {DepartmentId}: {Error}",
                departmentId,
                result.Error.ToResponse());

            return result.Error.ToResponse();
        }

        _logger.LogInformation(
            "Локации подразделения {DepartmentId} успешно обновлены",
            result.Value.DepartmentId);

        return Ok(Envelope.Ok(result.Value));
    }

    [HttpPut("{departmentId:guid}/parent")]
    public async Task<IActionResult> MoveDepartmentAsync(
        [FromRoute] Guid departmentId,
        [FromBody] MoveDepartmentRequest request,
        CancellationToken cancellationToken = default)
    {
        var command = new MoveDepartmentCommand(departmentId, request);

        var result = await _moveHandler.HandleAsync(command, cancellationToken);
        if (result.IsFailure)
        {
            _logger.LogError("Ошибка переноса подразделения {DepartmentId}. " +
                          "Ошибка: {Error}", command.DepartmentId, result.Error.ToResponse());
            
            return result.Error.ToResponse();
        }

        _logger.LogInformation("Подразделене успешно перенесено.");
        
        return Ok(Envelope.Ok(result.Value));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteAsync(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var command = new DeleteDepartmentCommand(id);

        var result = await _deleteHandler.HandleAsync(command, cancellationToken);
        if (result.IsFailure)
        {
            return result.Error.ToResponse();
        }

        return Ok(Envelope.Ok(result.Value));
    }
    
    [HttpPost("{deptId:guid}/positions/{posId:guid}")]
    public async Task<IActionResult> AttachPositionToDepartment(
        [FromRoute] Guid deptId, 
        [FromRoute] Guid posId,
        CancellationToken cancellationToken)
    {
        var command = new AttachPositionToDepartmentCommand(deptId, posId);
        var result = await _attachPositionHandler.HandleAsync(command, cancellationToken);
        if (result.IsFailure)
            return result.Error.ToResponse();
    
        return Ok(Envelope.Ok(result.Value));
    }

    [HttpDelete("{deptId:guid}/positions/{posId:guid}")]
    public async Task<IActionResult> DetachPositionFromDepartment(
        [FromRoute] Guid deptId, 
        [FromRoute] Guid posId,
        CancellationToken cancellationToken)
    {
        var command = new DetachPositionFromDepartmentCommand(deptId, posId);
        var result = await _detachPositionHandler.HandleAsync(command, cancellationToken);
        if (result.IsFailure)
            return result.Error.ToResponse();

        return Ok(Envelope.Ok(result.Value));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetByIdAsync(
        [FromRoute] Guid id,
        CancellationToken cancellationToken = default)
    {
        var query = new GetDepartmentByIdQuery(id);
        var result = await _getByIdHandler.HandleAsync(query, cancellationToken);
    
        if (result is null)  // проверяем null вместо IsFailure
        {
            _logger.LogWarning("Отдел {DepartmentId} не найден.", id);
            return NotFound(Envelope.Error(
                Errors.General.NotFound(id)));
        }
    
        _logger.LogInformation("Отдел {DepartmentId} получено успешно.", id);
        return Ok(Envelope.Ok(result));
    }

    [HttpGet]
    public async Task<IActionResult> GetByFilterAsync(
        [FromQuery] GetDepartmentsRequest request,
        CancellationToken cancellationToken = default)
    {
        var query = new GetDepartmentsQuery(request);

        var result = await _getByFilterHandler.HandleAsync(query, cancellationToken);
        
        _logger.LogInformation("Отделы получены успешно.");
        return Ok(Envelope.Ok(result));
    }

    [HttpPut("{id:guid}/restore")]
    public async Task<IActionResult> RestoreAsync(
        [FromRoute] Guid id,
        CancellationToken cancellationToken = default)
    {
        var command = new RestoreDepartmentCommand(id);

        var result = await _restoreHandler.HandleAsync(command, cancellationToken);

        if (result.IsFailure)
        {
            return result.Error.ToResponse();
        }

        return Ok(Envelope.Ok(result.Value));
    }

    [HttpGet("tree")]
    public async Task<IActionResult> GetTreeAsync(
        CancellationToken cancellationToken = default)
    {
        var query = new GetDepartmentsTreeQuery();
        
        var response = await _getTreeHandler.HandleAsync(query, cancellationToken);

        if (response.Count == 0)
            return NotFound();
        
        _logger.LogInformation("Root-подразделения получены успешно.");
        return Ok(Envelope.Ok(response));
    }

    [HttpGet("{departmentId:guid}/children")]
    public async Task<IActionResult> GetChildrenAsync(
        [FromRoute] Guid departmentId,
        CancellationToken cancellationToken = default)
    {
        var query = new GetDepartmentsChildrenQuery(departmentId);

        var response = await _getChildrenHandler.HandleAsync(query, cancellationToken);
        
        if (response is null)
            return NotFound();
        
        _logger.LogInformation("Дети root-подразделения получены успешно.");
        return Ok(Envelope.Ok(response));
    }

    [HttpGet("{departmentId:guid}/ancestors")]
    public async Task<IActionResult> GetAncestorsAsync(
        [FromRoute] Guid departmentId,
        CancellationToken cancellationToken = default)
    {
        var query = new GetDepartmentsAncestorsQuery(departmentId);

        var response = await _getAncestorsHandler.HandleAsync(query, cancellationToken);

        if (response is null)
            return NotFound("Такого подразделения не существует.");
        
        _logger.LogInformation("Предки подразделения получены успешно.");
        return Ok(Envelope.Ok(response));
    }

    [HttpGet("tree/search")]
    public async Task<IActionResult> GetTreeSearchAsync(
        [FromQuery] string q,
        CancellationToken cancellationToken = default)
    {
        if (q.Length < 2)
            return BadRequest("Поисковая строка должна быть не менее двух символов");
        
        var query = new GetDepartmentsTreeSearchQuery(q);

        var response = await _getTreeSearchHandler.HandleAsync(query, cancellationToken);
        
        _logger.LogInformation("Поиск подразделений выполнен.");
        return Ok(Envelope.Ok(response));
    }
}