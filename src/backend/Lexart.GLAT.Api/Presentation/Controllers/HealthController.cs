using Lexart.GLAT.Api.Application.DTOs;
using Lexart.GLAT.Api.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Lexart.GLAT.Api.Presentation.Controllers;

[ApiController]
[Route("api/v1/health")]
public class HealthController : ControllerBase
{
    private readonly IHealthScoreService _healthScoreService;
    private readonly ILogger<HealthController> _logger;

    public HealthController(IHealthScoreService healthScoreService, ILogger<HealthController> logger)
    {
        _healthScoreService = healthScoreService;
        _logger = logger;
    }

    [HttpGet]
    [ProducesResponseType(typeof(SystemHealthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(SystemHealthResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> HealthCheck()
    {
        try
        {
            var result = await _healthScoreService.GetSystemHealthAsync();
            var statusCode = result.Status == "HEALTHY" ? StatusCodes.Status200OK : StatusCodes.Status500InternalServerError;
            return StatusCode(statusCode, result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking system health");
            var response = new SystemHealthResponse
            {
                Status = "UNHEALTHY",
                Timestamp = DateTime.UtcNow,
                Uptime = 0,
                Database = new DatabaseHealth { Status = "DISCONNECTED" }
            };
            return StatusCode(StatusCodes.Status500InternalServerError, response);
        }
    }
}

[ApiController]
[Route("api/v1/assets/{assetId}/health")]
[Authorize]
public class AssetHealthController : ControllerBase
{
    private readonly IHealthScoreService _healthScoreService;
    private readonly ILogger<AssetHealthController> _logger;

    public AssetHealthController(IHealthScoreService healthScoreService, ILogger<AssetHealthController> logger)
    {
        _healthScoreService = healthScoreService;
        _logger = logger;
    }

    [HttpGet]
    [ProducesResponseType(typeof(HealthScoreResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAssetHealth([FromRoute] Guid assetId)
    {
        try
        {
            var result = await _healthScoreService.GetAssetHealthScoreAsync(assetId);

            if (result == null)
            {
                var error = new ErrorResponse
                {
                    Error = "Asset or sensor data not found",
                    StatusCode = StatusCodes.Status404NotFound,
                    Timestamp = DateTime.UtcNow
                };
                return NotFound(error);
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating health score for asset {AssetId}", assetId);
            var error = new ErrorResponse
            {
                Error = "Error calculating health score",
                StatusCode = StatusCodes.Status500InternalServerError,
                Details = ex.Message,
                Timestamp = DateTime.UtcNow
            };
            return StatusCode(StatusCodes.Status500InternalServerError, error);
        }
    }

    [HttpGet("maintenance")]
    [ProducesResponseType(typeof(AssetMaintenanceResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAssetMaintenance([FromRoute] Guid assetId)
    {
        try
        {
            var result = await _healthScoreService.GetAssetMaintenanceAsync(assetId);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            var error = new ErrorResponse
            {
                Error = ex.Message,
                StatusCode = StatusCodes.Status404NotFound,
                Timestamp = DateTime.UtcNow
            };
            return NotFound(error);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving maintenance information for asset {AssetId}", assetId);
            var error = new ErrorResponse
            {
                Error = "Error retrieving maintenance information",
                StatusCode = StatusCodes.Status500InternalServerError,
                Details = ex.Message,
                Timestamp = DateTime.UtcNow
            };
            return StatusCode(StatusCodes.Status500InternalServerError, error);
        }
    }
}
