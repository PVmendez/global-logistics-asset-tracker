using Lexart.GLAT.Api.Application.DTOs;
using Lexart.GLAT.Api.Domain.Entities;
using Lexart.GLAT.Api.Infrastructure.Data;
using Lexart.GLAT.Api.Infrastructure.Native;
using Microsoft.EntityFrameworkCore;

namespace Lexart.GLAT.Api.Application.Services;

public interface IHealthScoreService
{
    Task<HealthScoreResponse?> GetAssetHealthScoreAsync(Guid assetId);
    Task<SystemHealthResponse> GetSystemHealthAsync();
    Task<AssetMaintenanceResponse> GetAssetMaintenanceAsync(Guid assetId);
}

public class HealthScoreService : IHealthScoreService
{
    private readonly GlatDbContext _context;
    private readonly ILogger<HealthScoreService> _logger;
    private readonly DateTime _startTime = DateTime.UtcNow;

    public HealthScoreService(GlatDbContext context, ILogger<HealthScoreService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<HealthScoreResponse?> GetAssetHealthScoreAsync(Guid assetId)
    {
        try
        {
            var latestTelemetry = await _context.TelemetryLogs
                .Where(t => t.AssetId == assetId)
                .OrderByDescending(t => t.Timestamp)
                .FirstOrDefaultAsync();

            if (latestTelemetry?.SensorStatus == null)
                return null;

            var result = HealthCalculatorInterop.CalculateAssetHealth(
                latestTelemetry.SensorStatus.Temperature,
                latestTelemetry.SensorStatus.Humidity,
                latestTelemetry.SensorStatus.Pressure,
                latestTelemetry.SensorStatus.Vibration,
                latestTelemetry.SensorStatus.BatteryLevel ?? -1
            );

            if (result == null)
            {
                _logger.LogWarning("Health calculation failed for asset {AssetId}", assetId);
                return null;
            }

            var (score, factors) = result.Value;
            var status = HealthCalculatorInterop.GetHealthStatus(score);

            return new HealthScoreResponse
            {
                AssetId = assetId,
                Score = score,
                Status = status,
                LastUpdate = latestTelemetry.Timestamp,
                Factors = new HealthFactors
                {
                    Temperature = factors["temperature"],
                    Humidity = factors["humidity"],
                    Pressure = factors["pressure"],
                    Vibration = factors["vibration"]
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating health score for asset {AssetId}", assetId);
            throw;
        }
    }

    public async Task<SystemHealthResponse> GetSystemHealthAsync()
    {
        try
        {
            var uptime = (long)(DateTime.UtcNow - _startTime).TotalSeconds;

            var dbHealth = new DatabaseHealth { Status = "DISCONNECTED", Latency = null };
            try
            {
                var startTick = System.Diagnostics.Stopwatch.StartNew();
                await _context.Database.ExecuteSqlAsync($"SELECT 1");
                startTick.Stop();
                dbHealth.Status = "CONNECTED";
                dbHealth.Latency = startTick.ElapsedMilliseconds;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Database health check failed");
            }

            var status = dbHealth.Status == "CONNECTED" ? "HEALTHY" : "DEGRADED";

            return new SystemHealthResponse
            {
                Status = status,
                Timestamp = DateTime.UtcNow,
                Uptime = uptime,
                Database = dbHealth
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking system health");
            return new SystemHealthResponse
            {
                Status = "UNHEALTHY",
                Timestamp = DateTime.UtcNow,
                Uptime = (long)(DateTime.UtcNow - _startTime).TotalSeconds,
                Database = new DatabaseHealth { Status = "DISCONNECTED" }
            };
        }
    }

    public async Task<AssetMaintenanceResponse> GetAssetMaintenanceAsync(Guid assetId)
    {
        try
        {
            var asset = await _context.Assets.FindAsync(assetId);
            if (asset == null)
                throw new InvalidOperationException($"Asset with ID {assetId} not found");

            var alerts = new List<MaintenanceAlert>();
            var requiresMaintenance = false;
            var today = DateTime.UtcNow.Date;

            if (asset.LastMaintenanceDate.HasValue)
            {
                var daysSinceLastMaintenance = (today - asset.LastMaintenanceDate.Value.Date).Days;
                if (daysSinceLastMaintenance > 90)
                {
                    alerts.Add(new MaintenanceAlert
                    {
                        AlertType = "OVERDUE",
                        Message = $"Maintenance is overdue by {daysSinceLastMaintenance - 90} days",
                        DueDate = asset.LastMaintenanceDate?.AddDays(90),
                        DaysRemaining = 0,
                        Severity = "CRITICAL"
                    });
                    requiresMaintenance = true;
                }
            }

            if (asset.NextMaintenanceDate.HasValue)
            {
                var daysUntilMaintenance = (asset.NextMaintenanceDate.Value.Date - today).Days;
                if (daysUntilMaintenance >= 0 && daysUntilMaintenance <= 30)
                {
                    alerts.Add(new MaintenanceAlert
                    {
                        AlertType = "SCHEDULED",
                        Message = $"Maintenance is scheduled in {daysUntilMaintenance} days",
                        DueDate = asset.NextMaintenanceDate,
                        DaysRemaining = daysUntilMaintenance,
                        Severity = daysUntilMaintenance <= 7 ? "WARNING" : "INFO"
                    });
                    if (daysUntilMaintenance == 0)
                        requiresMaintenance = true;
                }
            }

            var healthScore = await GetAssetHealthScoreAsync(assetId);
            if (healthScore != null && healthScore.Score < 60)
            {
                alerts.Add(new MaintenanceAlert
                {
                    AlertType = "LOW_HEALTH",
                    Message = $"Asset health score is low ({healthScore.Score.ToString("F1")}%)",
                    DueDate = DateTime.UtcNow,
                    DaysRemaining = 0,
                    Severity = "WARNING"
                });
                requiresMaintenance = true;
            }

            return new AssetMaintenanceResponse
            {
                AssetId = assetId,
                LastMaintenanceDate = asset.LastMaintenanceDate,
                NextMaintenanceDate = asset.NextMaintenanceDate,
                Alerts = alerts,
                RequiresMaintenance = requiresMaintenance
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving maintenance information for asset {AssetId}", assetId);
            throw;
        }
    }
}
