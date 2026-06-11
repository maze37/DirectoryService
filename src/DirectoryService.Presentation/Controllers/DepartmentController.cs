using DirectoryService.Application.UseCases.DepartmentCases.AttachPositionToDepartment;
using DirectoryService.Application.UseCases.DepartmentCases.CreateDepartment;
using DirectoryService.Application.UseCases.DepartmentCases.DeleteDepartment;
using DirectoryService.Application.UseCases.DepartmentCases.DetachPositionFromDepartment;
using DirectoryService.Application.UseCases.DepartmentCases.MoveDepartment;
using DirectoryService.Application.UseCases.DepartmentCases.UpdateDepartmentLocations;
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
    private readonly ILogger<DepartmentController> _logger;

    public DepartmentController(
        ICommandHandler<CreateDepartmentCommand, CreateDepartmentResponse> createHandler,
        ICommandHandler<UpdateDepartmentLocationsCommand, UpdateDepartmentLocationsResponse> updateLocationsHandler,
        ICommandHandler<MoveDepartmentCommand, MoveDepartmentResponse> moveHandler,
        ICommandHandler<DeleteDepartmentCommand, DeleteDepartmentResponse> deleteHandler,
        ICommandHandler<AttachPositionToDepartmentCommand, AttachPositionToDepartmentResponse> attachPositionHandler,
        ICommandHandler<DetachPositionFromDepartmentCommand, DetachPositionFromDepartmentResponse> detachPositionHandler,
        ILogger<DepartmentController> logger)
    {
        _createHandler = createHandler;
        _updateLocationsHandler = updateLocationsHandler;
        _moveHandler = moveHandler;
        _deleteHandler = deleteHandler;
        _attachPositionHandler = attachPositionHandler;
        _detachPositionHandler = detachPositionHandler;
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
}