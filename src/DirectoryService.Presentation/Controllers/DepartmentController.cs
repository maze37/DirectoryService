using DirectoryService.Application.UseCases.DepartmentCases.CreateDepartment;
using DirectoryService.Application.UseCases.DepartmentCases.MoveDepartment;
using DirectoryService.Application.UseCases.DepartmentCases.UpdateDepartmentLocations;
using DirectoryService.Contracts.DepartmentContracts;
using DirectoryService.Presentation.ResponseExtensions;
using Microsoft.AspNetCore.Mvc;
using Shared.Core;
using Shared.Result;
using ILogger = Serilog.ILogger;

namespace DirectoryService.Presentation.Controllers;

[ApiController]
[Route("api/departments")]
public class DepartmentController : ControllerBase
{
    private readonly ICommandHandler<CreateDepartmentCommand, CreateDepartmentResponse> _createHandler;
    private readonly ICommandHandler<UpdateDepartmentLocationsCommand, UpdateDepartmentLocationsResponse> _updateLocationsHandler;
    private readonly ICommandHandler<MoveDepartmentCommand, MoveDepartmentResponse> _moveHandler;
    private readonly ILogger _logger;

    public DepartmentController(
        ICommandHandler<CreateDepartmentCommand, CreateDepartmentResponse> createHandler,
        ICommandHandler<UpdateDepartmentLocationsCommand, UpdateDepartmentLocationsResponse> updateLocationsHandler,
        ICommandHandler<MoveDepartmentCommand, MoveDepartmentResponse> moveHandler,
        ILogger logger)
    {
        _createHandler = createHandler;
        _updateLocationsHandler = updateLocationsHandler;
        _moveHandler = moveHandler;
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
            _logger.Error("Ошибка создания отдела: {Error}", result.Error.ToResponse());
            return result.Error.ToResponse();
        }

        _logger.Information("Отдел с ID: {DepartmentId} успешно создана", result.Value.Id);

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
            _logger.Error(
                "Ошибка обновления локаций подразделения {DepartmentId}: {Error}",
                departmentId,
                result.Error.ToResponse());

            return result.Error.ToResponse();
        }

        _logger.Information(
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
            _logger.Error("Ошибка переноса подразделения {DepartmentId}. " +
                          "Ошибка: {Error}", command.DepartmentId, result.Error.ToResponse());
            
            return result.Error.ToResponse();
        }

        _logger.Information("Подразделене успешно перенесено.");
        
        return Ok(Envelope.Ok(result.Value));
    }
}