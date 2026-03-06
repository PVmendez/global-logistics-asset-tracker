using AutoMapper;
using Lexart.GLAT.Api.Application.DTOs;
using Lexart.GLAT.Api.Domain.Entities;
using Lexart.GLAT.Api.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Lexart.GLAT.Api.Application.Services;

public interface ITelemetryService
{
    Task<TelemetryLogDto?> GetTelemetryByIdAsync(Guid telemetryId);
    Task<TelemetryListResponse> GetAssetTelemetryAsync(Guid assetId, int page, int pageSize, 
        DateTime? startTime = null, DateTime? endTime = null);
    Task<TelemetryLogDto> LogTelemetryAsync(Guid assetId, CreateTelemetryRequest request);
}

public class TelemetryService : ITelemetryService
{
    private readonly GlatDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<TelemetryService> _logger;

    public TelemetryService(GlatDbContext context, IMapper mapper, ILogger<TelemetryService> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<TelemetryLogDto?> GetTelemetryByIdAsync(Guid telemetryId)
    {
        try
        {
            var log = await _context.TelemetryLogs.FindAsync(telemetryId);
            return log == null ? null : _mapper.Map<TelemetryLogDto>(log);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving telemetry with ID {TelemetryId}", telemetryId);
            throw;
        }
    }

    public async Task<TelemetryListResponse> GetAssetTelemetryAsync(Guid assetId, int page, int pageSize,
        DateTime? startTime = null, DateTime? endTime = null)
    {
        try
        {
            var assetExists = await _context.Assets.AnyAsync(a => a.Id == assetId);
            if (!assetExists)
                throw new InvalidOperationException($"Asset with ID {assetId} not found");

            var query = _context.TelemetryLogs
                .Where(t => t.AssetId == assetId)
                .AsQueryable();

            if (startTime.HasValue)
                query = query.Where(t => t.Timestamp >= startTime.Value);
            if (endTime.HasValue)
                query = query.Where(t => t.Timestamp <= endTime.Value);

            var total = await query.CountAsync();
            var items = await query
                .OrderByDescending(t => t.Timestamp)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new TelemetryListResponse
            {
                Items = _mapper.Map<List<TelemetryLogDto>>(items),
                Total = total,
                Page = page,
                PageSize = pageSize,
                TotalPages = (total + pageSize - 1) / pageSize
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving telemetry for asset {AssetId}", assetId);
            throw;
        }
    }

    public async Task<TelemetryLogDto> LogTelemetryAsync(Guid assetId, CreateTelemetryRequest request)
    {
        try
        {
            var asset = await _context.Assets.FindAsync(assetId);
            if (asset == null)
                throw new InvalidOperationException($"Asset with ID {assetId} not found");

            var telemetryLog = new TelemetryLog
            {
                Id = Guid.NewGuid(),
                AssetId = assetId,
                Timestamp = request.Timestamp,
                SensorStatus = new SensorStatus
                {
                    Temperature = request.SensorStatus.Temperature,
                    Humidity = request.SensorStatus.Humidity,
                    Pressure = request.SensorStatus.Pressure,
                    Vibration = request.SensorStatus.Vibration,
                    BatteryLevel = request.SensorStatus.BatteryLevel,
                    Gps = request.SensorStatus.Gps == null ? null : new GpsCoordinates
                    {
                        Latitude = request.SensorStatus.Gps.Latitude,
                        Longitude = request.SensorStatus.Gps.Longitude,
                        Altitude = request.SensorStatus.Gps.Altitude
                    }
                },
                CreatedAt = DateTime.UtcNow
            };

            _context.TelemetryLogs.Add(telemetryLog);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Telemetry logged for asset {AssetId}", assetId);
            return _mapper.Map<TelemetryLogDto>(telemetryLog);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error logging telemetry for asset {AssetId}", assetId);
            throw;
        }
    }
}
