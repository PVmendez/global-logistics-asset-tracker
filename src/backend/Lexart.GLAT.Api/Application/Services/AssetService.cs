using AutoMapper;
using Lexart.GLAT.Api.Application.DTOs;
using Lexart.GLAT.Api.Domain.Entities;
using Lexart.GLAT.Api.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Lexart.GLAT.Api.Application.Services;

public interface IAssetService
{
    Task<AssetDto?> GetAssetByIdAsync(Guid assetId);
    Task<AssetListResponse> GetAssetsAsync(int page, int pageSize, string? status = null, string? type = null);
    Task<AssetDto> CreateAssetAsync(CreateAssetRequest request);
    Task<AssetDto?> UpdateAssetAsync(Guid assetId, UpdateAssetRequest request);
    Task<bool> DeleteAssetAsync(Guid assetId);
}

public class AssetService : IAssetService
{
    private readonly GlatDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<AssetService> _logger;
    private readonly IHealthScoreService _healthScoreService;

    public AssetService(GlatDbContext context, IMapper mapper, ILogger<AssetService> logger, IHealthScoreService healthScoreService)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
        _healthScoreService = healthScoreService;
    }

    public async Task<AssetDto?> GetAssetByIdAsync(Guid assetId)
    {
        try
        {
            var asset = await _context.Assets.FindAsync(assetId);
            if (asset == null) return null;

            var dto = _mapper.Map<AssetDto>(asset);

            var healthScore = await _healthScoreService.GetAssetHealthScoreAsync(assetId);
            if (healthScore != null)
            {
                dto.HealthScore = healthScore.Score;
            }

            return dto;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving asset with ID {AssetId}", assetId);
            throw;
        }
    }

    public async Task<AssetListResponse> GetAssetsAsync(int page, int pageSize, string? status = null, string? type = null)
    {
        try
        {
            var query = _context.Assets.AsQueryable();

            if (!string.IsNullOrEmpty(status))
            {
                if (Enum.TryParse<AssetStatus>(status, out var assetStatus))
                {
                    query = query.Where(a => a.Status == assetStatus);
                }
            }

            if (!string.IsNullOrEmpty(type))
            {
                if (Enum.TryParse<AssetType>(type, out var assetType))
                {
                    query = query.Where(a => a.Type == assetType);
                }
            }

            var total = await query.CountAsync();
            var items = await query
                .OrderByDescending(a => a.UpdatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var dtos = _mapper.Map<List<AssetDto>>(items);

            foreach (var dto in dtos)
            {
                var healthScore = await _healthScoreService.GetAssetHealthScoreAsync(dto.Id);
                if (healthScore != null)
                {
                    dto.HealthScore = healthScore.Score;
                }
            }

            return new AssetListResponse
            {
                Items = dtos,
                Total = total,
                Page = page,
                PageSize = pageSize,
                TotalPages = (total + pageSize - 1) / pageSize
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving assets");
            throw;
        }
    }

    public async Task<AssetDto> CreateAssetAsync(CreateAssetRequest request)
    {
        try
        {
            var existing = await _context.Assets
                .FirstOrDefaultAsync(a => a.SerialNumber == request.SerialNumber);

            if (existing != null)
            {
                throw new InvalidOperationException($"Asset with serial number {request.SerialNumber} already exists");
            }

            var asset = new Asset
            {
                Id = Guid.NewGuid(),
                SerialNumber = request.SerialNumber,
                Type = Enum.Parse<AssetType>(request.Type),
                Status = Enum.Parse<AssetStatus>(request.Status),
                Description = request.Description,
                Location = request.Location,
                LastMaintenanceDate = request.LastMaintenanceDate,
                NextMaintenanceDate = request.NextMaintenanceDate,
                Owner = request.Owner,
                Metadata = request.Metadata,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Assets.Add(asset);
            await _context.SaveChangesAsync();

            var initialTelemetry = new TelemetryLog
            {
                Id = Guid.NewGuid(),
                AssetId = asset.Id,
                Timestamp = DateTime.UtcNow,
                SensorStatus = new SensorStatus
                {
                    Temperature = 20f,
                    Humidity = 50f,
                    Pressure = 15f,
                    Vibration = 1f,
                    BatteryLevel = 85f
                },
                CreatedAt = DateTime.UtcNow
            };

            _context.TelemetryLogs.Add(initialTelemetry);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Asset created with ID {AssetId} and initial telemetry", asset.Id);

            var dto = _mapper.Map<AssetDto>(asset);
            var healthScore = await _healthScoreService.GetAssetHealthScoreAsync(asset.Id);
            if (healthScore != null)
            {
                dto.HealthScore = healthScore.Score;
            }
            return dto;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating asset");
            throw;
        }
    }

    public async Task<AssetDto?> UpdateAssetAsync(Guid assetId, UpdateAssetRequest request)
    {
        try
        {
            var asset = await _context.Assets.FindAsync(assetId);
            if (asset == null)
                return null;

            if (!string.IsNullOrEmpty(request.SerialNumber) && request.SerialNumber != asset.SerialNumber)
            {
                var existing = await _context.Assets
                    .FirstOrDefaultAsync(a => a.SerialNumber == request.SerialNumber);
                if (existing != null)
                    throw new InvalidOperationException($"Serial number {request.SerialNumber} is already in use");
            }

            if (!string.IsNullOrEmpty(request.SerialNumber))
                asset.SerialNumber = request.SerialNumber;
            if (!string.IsNullOrEmpty(request.Type))
                asset.Type = Enum.Parse<AssetType>(request.Type);
            if (!string.IsNullOrEmpty(request.Status))
                asset.Status = Enum.Parse<AssetStatus>(request.Status);
            if (request.Description != null)
                asset.Description = request.Description;
            if (!string.IsNullOrEmpty(request.Location))
                asset.Location = request.Location;
            if (request.LastMaintenanceDate.HasValue)
                asset.LastMaintenanceDate = request.LastMaintenanceDate;
            if (request.NextMaintenanceDate.HasValue)
                asset.NextMaintenanceDate = request.NextMaintenanceDate;
            if (request.Owner != null)
                asset.Owner = request.Owner;
            if (request.Metadata != null)
                asset.Metadata = request.Metadata;

            asset.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            _logger.LogInformation("Asset updated with ID {AssetId}", assetId);
            return _mapper.Map<AssetDto>(asset);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating asset with ID {AssetId}", assetId);
            throw;
        }
    }

    public async Task<bool> DeleteAssetAsync(Guid assetId)
    {
        try
        {
            var asset = await _context.Assets.FindAsync(assetId);
            if (asset == null)
                return false;

            _context.Assets.Remove(asset);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Asset deleted with ID {AssetId}", assetId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting asset with ID {AssetId}", assetId);
            throw;
        }
    }
}
