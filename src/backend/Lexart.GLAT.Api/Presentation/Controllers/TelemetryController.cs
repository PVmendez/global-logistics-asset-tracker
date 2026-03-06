using Lexart.GLAT.Api.Application.DTOs;
using Lexart.GLAT.Api.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Lexart.GLAT.Api.Presentation.Controllers;

[ApiController]
[Route("api/v1/assets/{assetId}/telemetry")]
[Authorize]
public class TelemetryController : ControllerBase
{
    private readonly ITelemetryService _telemetryService;
    private readonly ILogger<TelemetryController> _logger;

    public TelemetryController(ITelemetryService telemetryService, ILogger<TelemetryController> logger)
    {
        _telemetryService = telemetryService;
        _logger = logger;
    }

    [HttpGet]
    [ProducesResponseType(typeof(TelemetryListResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAssetTelemetry(
        [FromRoute] Guid assetId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        [FromQuery] DateTime? startTime = null,
        [FromQuery] DateTime? endTime = null)
    {
        try
        {
            if (page < 1 || pageSize < 1 || pageSize > 500)
            {
                var error = new ErrorResponse
                {
                    Error = "Invalid pagination parameters",
                    StatusCode = StatusCodes.Status400BadRequest,
                    Timestamp = DateTime.UtcNow
                };
                return BadRequest(error);
            }

            var result = await _telemetryService.GetAssetTelemetryAsync(assetId, page, pageSize, startTime, endTime);
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
            _logger.LogError(ex, "Error retrieving telemetry for asset {AssetId}", assetId);
            var error = new ErrorResponse
            {
                Error = "Error retrieving telemetry",
                StatusCode = StatusCodes.Status500InternalServerError,
                Details = ex.Message,
                Timestamp = DateTime.UtcNow
            };
            return StatusCode(StatusCodes.Status500InternalServerError, error);
        }
    }

    [HttpPost]
    [ProducesResponseType(typeof(TelemetryLogDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> LogAssetTelemetry(
        [FromRoute] Guid assetId,
        [FromBody] CreateTelemetryRequest request)
    {
        try
        {
            if (request.SensorStatus == null)
            {
                var error = new ErrorResponse
                {
                    Error = "SensorStatus is required",
                    StatusCode = StatusCodes.Status400BadRequest,
                    Timestamp = DateTime.UtcNow
                };
                return BadRequest(error);
            }

            var result = await _telemetryService.LogTelemetryAsync(assetId, request);
            return CreatedAtRoute(null, result);
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
            _logger.LogError(ex, "Error logging telemetry for asset {AssetId}", assetId);
            var error = new ErrorResponse
            {
                Error = "Error logging telemetry",
                StatusCode = StatusCodes.Status500InternalServerError,
                Details = ex.Message,
                Timestamp = DateTime.UtcNow
            };
            return StatusCode(StatusCodes.Status500InternalServerError, error);
        }
    }
}
