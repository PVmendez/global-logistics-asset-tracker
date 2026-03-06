using Lexart.GLAT.Api.Application.DTOs;
using Lexart.GLAT.Api.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Lexart.GLAT.Api.Presentation.Controllers;

[ApiController]
[Route("api/v1/assets")]
[Authorize]
public class AssetsController : ControllerBase
{
    private readonly IAssetService _assetService;
    private readonly ILogger<AssetsController> _logger;

    public AssetsController(IAssetService assetService, ILogger<AssetsController> logger)
    {
        _assetService = assetService;
        _logger = logger;
    }


    [HttpGet]
    [ProducesResponseType(typeof(AssetListResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAssets(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? status = null,
        [FromQuery] string? type = null)
    {
        try
        {
            if (page < 1 || pageSize < 1 || pageSize > 100)
            {
                var error = new ErrorResponse
                {
                    Error = "Invalid pagination parameters",
                    StatusCode = StatusCodes.Status400BadRequest,
                    Timestamp = DateTime.UtcNow
                };
                return BadRequest(error);
            }

            var result = await _assetService.GetAssetsAsync(page, pageSize, status, type);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving assets");
            var error = new ErrorResponse
            {
                Error = "Error retrieving assets",
                StatusCode = StatusCodes.Status500InternalServerError,
                Details = ex.Message,
                Timestamp = DateTime.UtcNow
            };
            return StatusCode(StatusCodes.Status500InternalServerError, error);
        }
    }

    [HttpGet("{assetId}")]
    [ProducesResponseType(typeof(AssetDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAssetById([FromRoute] Guid assetId)
    {
        try
        {
            var result = await _assetService.GetAssetByIdAsync(assetId);

            if (result == null)
            {
                var error = new ErrorResponse
                {
                    Error = "Asset not found",
                    StatusCode = StatusCodes.Status404NotFound,
                    Timestamp = DateTime.UtcNow
                };
                return NotFound(error);
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving asset {AssetId}", assetId);
            var error = new ErrorResponse
            {
                Error = "Error retrieving asset",
                StatusCode = StatusCodes.Status500InternalServerError,
                Details = ex.Message,
                Timestamp = DateTime.UtcNow
            };
            return StatusCode(StatusCodes.Status500InternalServerError, error);
        }
    }

    [HttpPost]
    [ProducesResponseType(typeof(AssetDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateAsset([FromBody] CreateAssetRequest request)
    {
        try
        {
            if (string.IsNullOrEmpty(request.SerialNumber) || string.IsNullOrEmpty(request.Location))
            {
                var error = new ErrorResponse
                {
                    Error = "SerialNumber and Location are required",
                    StatusCode = StatusCodes.Status400BadRequest,
                    Timestamp = DateTime.UtcNow
                };
                return BadRequest(error);
            }

            var result = await _assetService.CreateAssetAsync(request);
            return CreatedAtAction(nameof(GetAssetById), new { assetId = result.Id }, result);
        }
        catch (InvalidOperationException ex)
        {
            var error = new ErrorResponse
            {
                Error = ex.Message,
                StatusCode = StatusCodes.Status409Conflict,
                Timestamp = DateTime.UtcNow
            };
            return Conflict(error);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating asset");
            var error = new ErrorResponse
            {
                Error = "Error creating asset",
                StatusCode = StatusCodes.Status500InternalServerError,
                Details = ex.Message,
                Timestamp = DateTime.UtcNow
            };
            return StatusCode(StatusCodes.Status500InternalServerError, error);
        }
    }

    [HttpPut("{assetId}")]
    [ProducesResponseType(typeof(AssetDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateAsset([FromRoute] Guid assetId, [FromBody] UpdateAssetRequest request)
    {
        try
        {
            var result = await _assetService.UpdateAssetAsync(assetId, request);

            if (result == null)
            {
                var error = new ErrorResponse
                {
                    Error = "Asset not found",
                    StatusCode = StatusCodes.Status404NotFound,
                    Timestamp = DateTime.UtcNow
                };
                return NotFound(error);
            }

            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            var error = new ErrorResponse
            {
                Error = ex.Message,
                StatusCode = StatusCodes.Status409Conflict,
                Timestamp = DateTime.UtcNow
            };
            return Conflict(error);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating asset {AssetId}", assetId);
            var error = new ErrorResponse
            {
                Error = "Error updating asset",
                StatusCode = StatusCodes.Status500InternalServerError,
                Details = ex.Message,
                Timestamp = DateTime.UtcNow
            };
            return StatusCode(StatusCodes.Status500InternalServerError, error);
        }
    }

    [HttpDelete("{assetId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteAsset([FromRoute] Guid assetId)
    {
        try
        {
            var result = await _assetService.DeleteAssetAsync(assetId);

            if (!result)
            {
                var error = new ErrorResponse
                {
                    Error = "Asset not found",
                    StatusCode = StatusCodes.Status404NotFound,
                    Timestamp = DateTime.UtcNow
                };
                return NotFound(error);
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting asset {AssetId}", assetId);
            var error = new ErrorResponse
            {
                Error = "Error deleting asset",
                StatusCode = StatusCodes.Status500InternalServerError,
                Details = ex.Message,
                Timestamp = DateTime.UtcNow
            };
            return StatusCode(StatusCodes.Status500InternalServerError, error);
        }
    }
}
