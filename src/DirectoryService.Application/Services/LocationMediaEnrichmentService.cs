using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Contracts.LocationContracts;
using FileService.Contracts.Dtos;
using FileService.Contracts.HttpCommunication;
using Microsoft.Extensions.Logging;
using SharedKernel;

namespace DirectoryService.Application.Services;

public class LocationMediaEnrichmentService : ILocationMediaEnrichmentService
{
    private readonly IFileCommunicationService _fileCommunicationService;
    private readonly ILogger<LocationMediaEnrichmentService> _logger;

    public LocationMediaEnrichmentService(
        IFileCommunicationService fileCommunicationService,
        ILogger<LocationMediaEnrichmentService> logger)
    {
        _fileCommunicationService = fileCommunicationService;
        _logger = logger;
    }

    public async Task<MediaAssetDto> EnrichMediaAssetDtoAsync(
        Guid? photoAssetId,
        CancellationToken cancellationToken)
    {
        if (photoAssetId is null)
            return new MediaAssetDto(null, null, null);

        Result<GetFileResponse, Error> fileResult;

        try
        {
            fileResult = await _fileCommunicationService.GetMediaAsset(new GetFileRequest(photoAssetId.Value), cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Не удалось получить фото {PhotoAssetId} - File Service недоступен", photoAssetId);
            return new MediaAssetDto(photoAssetId, null, null);
        }

        if (fileResult.IsFailure)
        {
            _logger.LogWarning("Не удалось получить фото {PhotoAssetId}: {Error}", photoAssetId, fileResult.Error);
            return new MediaAssetDto(photoAssetId, null, null);
        }

        return new MediaAssetDto(photoAssetId, fileResult.Value.DownloadUrl, fileResult.Value.Status);
    }
}