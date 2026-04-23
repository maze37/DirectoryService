using DirectoryService.Application.UseCases.PositionCases.CreatePosition;
using DirectoryService.Contracts.PositionContracts;
using DirectoryService.Presentation.ResponseExtensions;
using Microsoft.AspNetCore.Mvc;
using Shared.Core;
using Shared.Result;
using ILogger = Serilog.ILogger;

namespace DirectoryService.Presentation.Controllers;

[ApiController]
[Route("api/positions")]
public class PositionController : ControllerBase
{
    private readonly ICommandHandler<CreatePositionCommand, CreatePositionResponse> _createHandler;
    private readonly ILogger _logger;

    public PositionController(
        ICommandHandler<CreatePositionCommand, CreatePositionResponse> createHandler,
        ILogger logger)
    {
        _createHandler = createHandler ?? throw new ArgumentNullException(nameof(createHandler));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
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
            _logger.Error("Ошибка создания позиции: {Error}", result.Error.ToResponse());
            return result.Error.ToResponse();
        }

        _logger.Information("Позиция с ID: {PositionId} успешно создана", result.Value.Id);
        return Ok(Envelope.Ok(result.Value));
    }
}
