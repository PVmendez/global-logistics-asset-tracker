namespace Lexart.GLAT.Api.Application.DTOs;

public class TelemetryLogDto
{
    public Guid Id { get; set; }
    public Guid AssetId { get; set; }
    public DateTime Timestamp { get; set; }
    public SensorStatusDto SensorStatus { get; set; } = null!;
    public float? HealthScore { get; set; }
    public List<AlertDto>? Alerts { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateTelemetryRequest
{
    public DateTime Timestamp { get; set; }
    public CreateSensorStatusRequest SensorStatus { get; set; } = null!;
}

public class SensorStatusDto
{
    public float Temperature { get; set; }
    public float Humidity { get; set; }
    public float Pressure { get; set; }
    public float Vibration { get; set; }
    public GpsCoordinatesDto? Gps { get; set; }
    public float? BatteryLevel { get; set; }
}

public class CreateSensorStatusRequest
{
    public float Temperature { get; set; }
    public float Humidity { get; set; }
    public float Pressure { get; set; }
    public float Vibration { get; set; }
    public GpsCoordinatesDto? Gps { get; set; }
    public float? BatteryLevel { get; set; }
}

public class GpsCoordinatesDto
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public double? Altitude { get; set; }
}

public class AlertDto
{
    public Guid Id { get; set; }
    public string Severity { get; set; } = null!;
    public string Message { get; set; } = null!;
    public string? Code { get; set; }
}

public class TelemetryListResponse
{
    public List<TelemetryLogDto> Items { get; set; } = new();
    public int Total { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}
