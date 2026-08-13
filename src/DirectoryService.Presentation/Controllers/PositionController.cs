using DirectoryService.Application.UseCases.PositionCases.Commands.CreatePosition;
using DirectoryService.Application.UseCases.PositionCases.Commands.DeletePosition;
using DirectoryService.Application.UseCases.PositionCases.Commands.RenamePosition;
using DirectoryService.Application.UseCases.PositionCases.Commands.RestorePosition;
using DirectoryService.Contracts.PositionContracts;
using DirectoryService.Presentation.ResponseExtensions;
using Microsoft.AspNetCore.Mvc;
using Shared.Core;
using Shared.Result;

namespace DirectoryService.Presentation.Controllers;

[ApiController]
[Route("api/positions")]
public class PositionController : ControllerBase
{
    private readonly ICommandHandler<CreatePositionCommand, CreatePositionResponse> _createHandler;
    private readonly ICommandHandler<RenamePositionCommand, RenamePositionResponse> _renameHandler;
    private readonly ICommandHandler<DeletePositionCommand, DeletePositionResponse> _deleteHandler;
    private readonly ICommandHandler<RestorePositionCommand, RestorePositionResponse> _restoreHandler;
    private readonly ILogger<PositionController> _logger;

    public PositionController(
        ICommandHandler<CreatePositionCommand, CreatePositionResponse> createHandler,
        ICommandHandler<RenamePositionCommand, RenamePositionResponse> renameHandler,
        ICommandHandler<DeletePositionCommand, DeletePositionResponse> deleteHandler,
        ICommandHandler<RestorePositionCommand, RestorePositionResponse> restoreHandler,
        ILogger<PositionController> logger)
    {
        _createHandler = createHandler;
        _renameHandler = renameHandler;
        _deleteHandler = deleteHandler;
        _restoreHandler = restoreHandler;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> CreateAsync(
        [FromBody] CreatePositionRequest request,
        CancellationToken cancellationToken = default)
    {
        var command = new CreatePositionCommand(request);
        var result = await _createHandler.HandleAsync(command, cancellationToken);

        if (result.IsFailure)
        {
            _logger.LogError("Ошибка создания позиции: {Error}", result.Error.ToResponse());
            return result.Error.ToResponse();
        }

        _logger.LogInformation("Позиция с ID: {PositionId} успешно создана", result.Value.Id);
        return Ok(Envelope.Ok(result.Value));
    }

    [HttpPatch("{id:guid}")]
    public async Task<IActionResult> RenameAsync(
        [FromRoute] Guid id,
        [FromBody] RenamePositionRequest request,
        CancellationToken cancellationToken)
    {
        var command = new RenamePositionCommand(id, request);

        var result = await _renameHandler.HandleAsync(command, cancellationToken);
        if (result.IsFailure)
        {
            return result.Error.ToResponse();
        }

        return Ok(Envelope.Ok(result.Value));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteAsync(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var command = new DeletePositionCommand(id);

        var result = await _deleteHandler.HandleAsync(command, cancellationToken);
        if (result.IsFailure)
        {
            return result.Error.ToResponse();
        }

        return Ok(Envelope.Ok(result.Value));
    }
    
    [HttpPut]
    public async Task<IActionResult> UpdateAsync(
        [FromRoute] Guid id,
        CancellationToken cancellationToken = default)
    {
        var command = new RestorePositionCommand(id);

        var result = await _restoreHandler.HandleAsync(command, cancellationToken);

        if (result.IsFailure)
        {
            return result.Error.ToResponse();
        }

        return Ok(Envelope.Ok(result.Value));
    }
}
