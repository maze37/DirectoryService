using DirectoryService.Application.UseCases.DepartmentCases.CreateDepartment;
using DirectoryService.Contracts.DepartmentContracts;
using DirectoryService.Presentation.ResponseExtensions;
using Microsoft.AspNetCore.Mvc;
using Shared.Core;
using Shared.Result;
using ILogger = Serilog.ILogger;

namespace DirectoryService.Presentation.Controllers;

[ApiController]
[Route("api/departments")]
public class DepartmentControllers : ControllerBase
{
    private readonly ICommandHandler<CreateDepartmentCommand, CreateDepartmentResponse> _createHandler;
    private readonly ILogger _logger;
    
    public DepartmentControllers(
        ICommandHandler<CreateDepartmentCommand, CreateDepartmentResponse> createHandler,
        ILogger logger)
    {
        _createHandler = createHandler ?? throw new ArgumentNullException(nameof(createHandler));
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
}