using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Lexart.GLAT.Api.Application.DTOs;
using Lexart.GLAT.Api.Domain.Entities;
using Lexart.GLAT.Api.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace Lexart.GLAT.Api.Infrastructure.Security;

public interface IAuthenticationService
{
    Task<LoginResponse?> AuthenticateAsync(string username, string password);
    Task<User?> ValidateTokenAsync(string token);
}

public class AuthenticationService : IAuthenticationService
{
    private readonly GlatDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthenticationService> _logger;

    public AuthenticationService(GlatDbContext context, IConfiguration configuration, ILogger<AuthenticationService> logger)
    {
        _context = context;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<LoginResponse?> AuthenticateAsync(string username, string password)
    {
        try
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == username && u.IsActive);

            if (user == null)
            {
                _logger.LogWarning("Login attempt for non-existent user: {Username}", username);
                return null;
            }

            if (!VerifyPassword(password, user.PasswordHash))
            {
                _logger.LogWarning("Invalid password for user: {Username}", username);
                return null;
            }

            var token = GenerateJwtToken(user);

            _logger.LogInformation("User {Username} successfully authenticated", username);

            return new LoginResponse
            {
                AccessToken = token,
                TokenType = "Bearer",
                ExpiresIn = 3600
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error authenticating user {Username}", username);
            throw;
        }
    }

    public async Task<User?> ValidateTokenAsync(string token)
    {
        try
        {
            var jwtToken = new JwtSecurityTokenHandler().ReadToken(token) as JwtSecurityToken;
            if (jwtToken == null)
                return null;

            var userId = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var parsedUserId))
                return null;

            var user = await _context.Users.FindAsync(parsedUserId);
            return user?.IsActive == true ? user : null;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error validating token");
            return null;
        }
    }

    private string GenerateJwtToken(User user)
    {
        var jwtSecret = _configuration["Jwt:Secret"] ?? throw new InvalidOperationException("JWT Secret not configured");
        var jwtIssuer = _configuration["Jwt:Issuer"] ?? "glat-api";
        var jwtAudience = _configuration["Jwt:Audience"] ?? "glat-clients";

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Username),
            new(ClaimTypes.Email, user.Email),
            new("IssuedAt", DateTime.UtcNow.ToString("O"))
        };

        var token = new JwtSecurityToken(
            issuer: jwtIssuer,
            audience: jwtAudience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private bool VerifyPassword(string password, string hash)
    {
        return BCrypt.Net.BCrypt.Verify(password, hash);
    }
}
