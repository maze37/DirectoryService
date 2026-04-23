using DirectoryService.Application.UseCases.DepartmentCases.CreateDepartment;
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
    private readonly ILogger _logger;

    public DepartmentController(
        ICommandHandler<CreateDepartmentCommand, CreateDepartmentResponse> createHandler,
        ICommandHandler<UpdateDepartmentLocationsCommand, UpdateDepartmentLocationsResponse> updateLocationsHandler,
        ILogger logger)
    {
        _createHandler = createHandler ?? throw new ArgumentNullException(nameof(createHandler));
        _updateLocationsHandler = updateLocationsHandler ?? throw new ArgumentNullException(nameof(updateLocationsHandler));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
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
}