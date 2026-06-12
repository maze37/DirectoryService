using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Validation;
using DirectoryService.Contracts.LocationContracts;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Shared.Core;

namespace DirectoryService.Application.UseCases.LocationCases.DeleteLocation;

public class DeleteLocationCommandHandler : ICommandHandler<DeleteLocationCommand, DeleteLocationResponse>
{
    private readonly ILocationRepository _locationRepository;
    private readonly ITransactionManager _transactionManager;
    private readonly ILogger<DeleteLocationCommandHandler> _logger;
    private readonly IValidator<DeleteLocationCommand> _validator;

    public DeleteLocationCommandHandler(
        ILocationRepository locationRepository,
        ITransactionManager transactionManager,
        ILogger<DeleteLocationCommandHandler> logger,
        IValidator<DeleteLocationCommand> validator)
    {
        _locationRepository = locationRepository;
        _transactionManager = transactionManager;
        _logger = logger;
        _validator = validator;
    }

    public async Task<Result<DeleteLocationResponse, Error>> HandleAsync(
        DeleteLocationCommand command, 
        CancellationToken cancellationToken = default)
    {
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.ToError();
        }

        var transactionResult = await _transactionManager.BeginTransactionAsync(cancellationToken);
        if (transactionResult.IsFailure)
        {
            return transactionResult.Error;
        }

        using var transaction = transactionResult.Value;

        var locationResult = await _locationRepository.GetByIdWithLock(command.Id, cancellationToken);
        if (locationResult.IsFailure)
        {
            transaction.Rollback();
            return locationResult.Error;
        }
        
        var hasLinks = await _locationRepository.HasDepartmentLinksAsync(command.Id, cancellationToken);
        if (hasLinks)
            return Error.Conflict("location.has.links", "Локация привязана к подразделениям. Сначала отвяжите её.");

        _locationRepository.Remove(locationResult.Value);
        
        var saveResult = await _transactionManager.SaveChangesAsync(cancellationToken);
        if (saveResult.IsFailure)
        {
            transaction.Rollback();
            return saveResult.Error;
        }

        var commitResult = transaction.Commit();
        if (commitResult.IsFailure)
            return commitResult.Error;

        _logger.LogInformation("Локация {LocationId} удалена", command.Id);
        return new DeleteLocationResponse(command.Id);
    }
}