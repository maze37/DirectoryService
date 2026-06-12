using DirectoryService.Application.UseCases.LocationCases.Commands.CreateLocation;
using DirectoryService.Application.UseCases.LocationCases.Commands.DeleteLocation;
using DirectoryService.Application.UseCases.LocationCases.Queries.GetLocationById;
using DirectoryService.Contracts.LocationContracts;
using DirectoryService.Presentation.ResponseExtensions;
using Microsoft.AspNetCore.Mvc;
using Shared.Core;
using Shared.Result;
using ILogger = Serilog.ILogger;

namespace DirectoryService.Presentation.Controllers;

[ApiController]
[Route("api/locations")]
public class LocationController : ControllerBase
{
    private readonly ICommandHandler<CreateLocationCommand, CreateLocationResponse> _createHandler;
    private readonly ICommandHandler<DeleteLocationCommand, DeleteLocationResponse> _deleteHandler;
    private readonly IQueryHandler<GetLocationByIdQuery, GetLocationDto> _getByIdHandler;
    private readonly ILogger<LocationController> _logger;

    public LocationController(
        ICommandHandler<CreateLocationCommand, CreateLocationResponse> createHandler,
        ICommandHandler<DeleteLocationCommand, DeleteLocationResponse> deleteHandler,
        IQueryHandler<GetLocationByIdQuery, GetLocationDto> getByIdHandler,
        ILogger<LocationController> logger)
    {
        _createHandler = createHandler;
        _deleteHandler = deleteHandler;
        _getByIdHandler = getByIdHandler;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> CreateAsync(
        [FromBody] CreateLocationRequest location,
        CancellationToken cancellationToken = default)
    {
        var command = new CreateLocationCommand(location);

        var result = await _createHandler.HandleAsync(command, cancellationToken);

        if (result.IsFailure)
        {
            _logger.LogError("Ошибка создания локации: {Error}", result.Error.ToResponse());
            return result.Error.ToResponse();
        }

        _logger.LogInformation("Локация с ID: {LocationId} успешно создана", result.Value.Id);

        return Ok(Envelope.Ok(result.Value));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteAsync(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var command = new DeleteLocationCommand(id);

        var result = await _deleteHandler.HandleAsync(command, cancellationToken);
        if (result.IsFailure)
        {
            return result.Error.ToResponse();
        }

        return Ok(Envelope.Ok(result.Value));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetByIdAsync(
        [FromRoute] Guid id,
        CancellationToken cancellationToken = default)
    {
        var query = new GetLocationByIdQuery(id);

        var result = await _getByIdHandler.HandleAsync(query, cancellationToken);
        if (result is null)
        {
            _logger.LogWarning("Локация {LocationId} не найдена.", id);
            return NotFound(Envelope.Error(
                Errors.General.NotFound(id)));
        }
        
        _logger.LogInformation("Локация {LocationId} успешно получена.", id);

        return Ok(Envelope.Ok(result));
    }
}
