using Core.Abstractions;
using DirectoryService.Application.UseCases.LocationCases.Commands.AttachPhoto;
using DirectoryService.Application.UseCases.LocationCases.Commands.CreateLocation;
using DirectoryService.Application.UseCases.LocationCases.Commands.DeleteLocation;
using DirectoryService.Application.UseCases.LocationCases.Commands.RemovePhoto;
using DirectoryService.Application.UseCases.LocationCases.Commands.RestoreLocation;
using DirectoryService.Application.UseCases.LocationCases.Commands.UpdatePhoto;
using DirectoryService.Application.UseCases.LocationCases.Queries.GetLocationById;
using DirectoryService.Application.UseCases.LocationCases.Queries.GetLocations;
using DirectoryService.Application.UseCases.LocationCases.Queries.GetTopLocations;
using DirectoryService.Contracts;
using DirectoryService.Contracts.Constants;
using DirectoryService.Contracts.LocationContracts;
using Framework.ResponseExtensions;
using Microsoft.AspNetCore.Mvc;
using SharedKernel;

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
    private readonly ICommandHandler<AttachPhotoCommand, AttachPhotoResponse> _attachPhotoHandler;
    private readonly ICommandHandler<UpdatePhotoCommand, UpdatePhotoResponse> _updatePhotoHandler;
    private readonly ICommandHandler<RemovePhotoCommand, RemovePhotoResponse> _removePhotoHandler;
    private readonly ILogger<LocationController> _logger;

    public LocationController(
        ICommandHandler<CreateLocationCommand, CreateLocationResponse> createHandler,
        ICommandHandler<DeleteLocationCommand, DeleteLocationResponse> deleteHandler,
        ICommandHandler<RestoreLocationCommand, RestoreLocationResponse> restoreHandler,
        IQueryHandler<GetLocationByIdQuery, GetLocationDto> getByIdHandler,
        IQueryHandler<GetTopLocationsQuery, List<TopLocationDto>> getTopHandler,
        IQueryHandler<GetLocationsQuery, PagedResult<LocationListItemDto>> getLocationsHandler,
        ICommandHandler<AttachPhotoCommand, AttachPhotoResponse> attachPhotoHandler,
        ICommandHandler<UpdatePhotoCommand, UpdatePhotoResponse> updatePhotoHandler,
        ICommandHandler<RemovePhotoCommand, RemovePhotoResponse> removePhotoHandler,
        ILogger<LocationController> logger)
    {
        _createHandler = createHandler;
        _deleteHandler = deleteHandler;
        _restoreHandler = restoreHandler;
        _getByIdHandler = getByIdHandler;
        _getTopHandler = getTopHandler;
        _getLocationsHandler = getLocationsHandler;
        _attachPhotoHandler = attachPhotoHandler;
        _updatePhotoHandler = updatePhotoHandler;
        _removePhotoHandler = removePhotoHandler;
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
            return NotFound(Envelope.Fail(GeneralErrors.NotFound(id)));
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
    
    [HttpPut("{id:guid}/restore")]
    public async Task<IActionResult> RestoreAsync(
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

    [HttpPut("{locationId:guid}/attach-photo")]
    public async Task<IActionResult> AttachPhotoToLocation(
        [FromRoute] Guid locationId,
        AttachPhotoRequest request,
        CancellationToken cancellationToken)
    {
        var command = new AttachPhotoCommand(locationId, request);

        var response = await _attachPhotoHandler.HandleAsync(command, cancellationToken);
        if (response.IsFailure)
            return response.Error.ToResponse();
        
        return Ok(Envelope.Ok(response.Value));
    }
    
    [HttpPut("{locationId:guid}/update-photo")]
    public async Task<IActionResult> UpdatePhotoAsset(
        [FromRoute] Guid locationId,
        UpdatePhotoRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdatePhotoCommand(locationId, request);

        var response = await _updatePhotoHandler.HandleAsync(command, cancellationToken);
        if (response.IsFailure)
            return response.Error.ToResponse();

        return Ok(Envelope.Ok(response.Value));
    }
    
    [HttpPut("{locationId:guid}/remove-photo")]
    public async Task<IActionResult> RemovePhotoAsset(
        [FromRoute] Guid locationId,
        CancellationToken cancellationToken)
    {
        var command = new RemovePhotoCommand(locationId);

        var response = await _removePhotoHandler.HandleAsync(command, cancellationToken);
        if (response.IsFailure)
            return response.Error.ToResponse();
        
        return Ok(Envelope.Ok(response.Value));
    }
}
