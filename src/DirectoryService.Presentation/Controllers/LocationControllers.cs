using DirectoryService.Application.UseCases.LocationCases.CreateLocation;
using DirectoryService.Contracts.LocationContracts;
using DirectoryService.Presentation.ResponseExtensions;
using Microsoft.AspNetCore.Mvc;
using Shared.Core;

namespace DirectoryService.Presentation.Controllers;

[ApiController]
[Route("/api/locations")]
public class LocationControllers : ControllerBase
{
    private readonly ICommandHandler<CreateLocationCommand, CreateLocationResponse> _createHandler;

    public LocationControllers(ICommandHandler<CreateLocationCommand, CreateLocationResponse> createHandler)
    {
        _createHandler = createHandler ?? throw new ArgumentNullException(nameof(createHandler));
    }

    [HttpPost]
    public async Task<IActionResult> CreateAsync(
        [FromBody] CreateLocationRequest location,
        CancellationToken cancellationToken = default)
    {
        var command = new CreateLocationCommand(
            location.Name, 
            location.Address.Country,
            location.Address.City,
            location.Address.Street,
            location.Address.Building,
            location.Address.Office,
            location.Address.PostalCode,
            location.Timezone);

        var result = await _createHandler.HandleAsync(command, cancellationToken);

        if (result.IsFailure)
            return BadRequest(Envelope.Error(Error.Failure("failure", result.Error)));

        return Ok(Envelope.Ok(result.Value));
    }
}