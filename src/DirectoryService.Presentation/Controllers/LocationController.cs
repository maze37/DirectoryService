using System.Runtime.CompilerServices;
using DirectoryService.Application.UseCases.LocationCases.Commands.CreateLocation;
using DirectoryService.Application.UseCases.LocationCases.Commands.DeleteLocation;
using DirectoryService.Application.UseCases.LocationCases.Commands.RestoreLocation;
using DirectoryService.Application.UseCases.LocationCases.Queries.GetLocationById;
using DirectoryService.Application.UseCases.LocationCases.Queries.GetLocations;
using DirectoryService.Application.UseCases.LocationCases.Queries.GetTopLocations;
using DirectoryService.Application.UseCases.PositionCases.Commands.RestorePosition;
using DirectoryService.Contracts.Constants;
using DirectoryService.Contracts.LocationContracts;
using DirectoryService.Contracts.PositionContracts;
using DirectoryService.Presentation.ResponseExtensions;
using Microsoft.AspNetCore.Mvc;
using Shared.Core;
using Shared.Result;

namespace DirectoryService.Presentation.Controllers;

[ApiController]
[Route("api/locations")]
public class LocationController : ControllerBase
{
    private readonly ICommandHandler<CreateLocationCommand, CreateLocationResponse> _createHandler;
    private readonly ICommandHandler<DeleteLocationCommand, DeleteLocationResponse> _deleteHandler;
    private readonly ICommandHandler<RestoreLocationCommand, RestoreLocationResponse> _restoreHandler;
    private readonly IQueryHandler<GetLocationByIdQuery, GetLocationDto> _getByIdHandler;
    private readonly IQueryHandler<GetTopLocationsQuery, List<TopLocationDto>> _getTopHandler;
    private readonly IQueryHandler<GetLocationsQuery, PagedResult<LocationListItemDto>> _getLocationsHandler;
    private readonly ILogger<LocationController> _logger;

    public LocationController(
        ICommandHandler<CreateLocationCommand, CreateLocationResponse> createHandler,
        ICommandHandler<DeleteLocationCommand, DeleteLocationResponse> deleteHandler,
        ICommandHandler<RestoreLocationCommand, RestoreLocationResponse> restoreHandler,
        IQueryHandler<GetLocationByIdQuery, GetLocationDto> getByIdHandler,
        IQueryHandler<GetTopLocationsQuery, List<TopLocationDto>> getTopHandler,
        IQueryHandler<GetLocationsQuery, PagedResult<LocationListItemDto>> getLocationsHandler,
        ILogger<LocationController> logger)
    {
        _createHandler = createHandler;
        _deleteHandler = deleteHandler;
        _restoreHandler = restoreHandler;
        _getByIdHandler = getByIdHandler;
        _getTopHandler = getTopHandler;
        _getLocationsHandler = getLocationsHandler;
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

    [HttpGet("top")]
    public async Task<IActionResult> TopLocations(CancellationToken cancellationToken = default)
    {
        var query = new GetTopLocationsQuery();
        
        var result = await _getTopHandler.HandleAsync(query, cancellationToken);
        if (result is null)
        {
            // Если нет локаций вовсе, вернется null.
            _logger.LogWarning("Локаций в топе нет.");
        }
        
        _logger.LogInformation("Топ локаций получен. Количество: {Count}", result?.Count ?? 0);

        return Ok(Envelope.Ok(result));
    }

    [HttpGet]
    public async Task<IActionResult> GetLocationsAsync(
        [FromQuery] GetLocationsRequest request,
        CancellationToken cancellationToken = default)
    {
        var query = new GetLocationsQuery(request);

        var result = await _getLocationsHandler.HandleAsync(query, cancellationToken);
        
        return Ok(Envelope.Ok(result));
    }
    
    [HttpPut]
    public async Task<IActionResult> UpdateAsync(
        [FromRoute] Guid id,
        CancellationToken cancellationToken = default)
    {
        var command = new RestoreLocationCommand(id);

        var result = await _restoreHandler.HandleAsync(command, cancellationToken);

        if (result.IsFailure)
        {
            return result.Error.ToResponse();
        }

        return Ok(Envelope.Ok(result.Value));
    }
}
