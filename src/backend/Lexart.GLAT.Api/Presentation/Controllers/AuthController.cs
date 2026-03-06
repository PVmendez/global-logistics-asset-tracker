using Lexart.GLAT.Api.Application.DTOs;
using Lexart.GLAT.Api.Application.Services;
using Lexart.GLAT.Api.Infrastructure.Security;
using Microsoft.AspNetCore.Mvc;

namespace Lexart.GLAT.Api.Presentation.Controllers;

[ApiController]
[Route("api/v1/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthenticationService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthenticationService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password))
        {
            var error = new ErrorResponse
            {
                Error = "Username and password are required",
                StatusCode = StatusCodes.Status400BadRequest,
                Timestamp = DateTime.UtcNow
            };
            return BadRequest(error);
        }

        try
        {
            var response = await _authService.AuthenticateAsync(request.Username, request.Password);

            if (response == null)
            {
                var error = new ErrorResponse
                {
                    Error = "Invalid credentials",
                    StatusCode = StatusCodes.Status401Unauthorized,
                    Timestamp = DateTime.UtcNow
                };
                return Unauthorized(error);
            }

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Authentication error");
            var error = new ErrorResponse
            {
                Error = "An error occurred during authentication",
                StatusCode = StatusCodes.Status500InternalServerError,
                Details = ex.Message,
                Timestamp = DateTime.UtcNow
            };
            return StatusCode(StatusCodes.Status500InternalServerError, error);
        }
    }
}
