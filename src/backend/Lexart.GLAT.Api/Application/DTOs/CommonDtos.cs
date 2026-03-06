namespace Lexart.GLAT.Api.Application.DTOs;

public class HealthScoreResponse
{
    public Guid AssetId { get; set; }
    public float Score { get; set; }
    public string Status { get; set; } = null!;
    public DateTime LastUpdate { get; set; }
    public HealthFactors Factors { get; set; } = null!;
}

public class HealthFactors
{
    public float Temperature { get; set; }
    public float Humidity { get; set; }
    public float Pressure { get; set; }
    public float Vibration { get; set; }
}

public class SystemHealthResponse
{
    public string Status { get; set; } = null!;
    public DateTime Timestamp { get; set; }
    public long Uptime { get; set; }
    public DatabaseHealth? Database { get; set; }
}

public class DatabaseHealth
{
    public string Status { get; set; } = null!;
    public long? Latency { get; set; }
}

public class LoginRequest
{
    public string Username { get; set; } = null!;
    public string Password { get; set; } = null!;
}

public class LoginResponse
{
    public string AccessToken { get; set; } = null!;
    public string TokenType { get; set; } = "Bearer";
    public int ExpiresIn { get; set; }
    public string? RefreshToken { get; set; }
}

public class ErrorResponse
{
    public string Error { get; set; } = null!;
    public int StatusCode { get; set; }
    public string? Details { get; set; }
    public DateTime Timestamp { get; set; }
}
public class MaintenanceAlert
{
    public string AlertType { get; set; } = null!;
    public string Message { get; set; } = null!;
    public DateTime? DueDate { get; set; }
    public int DaysRemaining { get; set; }
    public string Severity { get; set; } = null!;
}

public class AssetMaintenanceResponse
{
    public Guid AssetId { get; set; }
    public DateTime? LastMaintenanceDate { get; set; }
    public DateTime? NextMaintenanceDate { get; set; }
    public List<MaintenanceAlert> Alerts { get; set; } = new();
    public bool RequiresMaintenance { get; set; }
}