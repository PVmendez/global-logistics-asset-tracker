namespace Lexart.GLAT.Api.Domain.Entities;

public class TelemetryLog
{
    public Guid Id { get; set; }
    public Guid AssetId { get; set; }
    public DateTime Timestamp { get; set; }
    public SensorStatus SensorStatus { get; set; } = null!;
    public float? HealthScore { get; set; }
    public List<Alert>? Alerts { get; set; }
    public DateTime CreatedAt { get; set; }

    public virtual Asset? Asset { get; set; }
}

public class SensorStatus
{
    public float Temperature { get; set; }
    public float Humidity { get; set; }
    public float Pressure { get; set; }
    public float Vibration { get; set; }
    public GpsCoordinates? Gps { get; set; }
    public float? BatteryLevel { get; set; }
}

public class GpsCoordinates
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public double? Altitude { get; set; }
}

public class Alert
{
    public Guid Id { get; set; }
    public AlertSeverity Severity { get; set; }
    public string Message { get; set; } = null!;
    public string? Code { get; set; }
}

public enum AlertSeverity
{
    INFO = 0,
    WARNING = 1,
    CRITICAL = 2
}
