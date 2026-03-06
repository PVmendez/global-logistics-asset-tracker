namespace Lexart.GLAT.Api.Domain.Entities;

public class Asset
{
    public Guid Id { get; set; }
    public string SerialNumber { get; set; } = null!;
    public AssetType Type { get; set; }
    public AssetStatus Status { get; set; }
    public string? Description { get; set; }
    public string Location { get; set; } = null!;
    public DateTime? LastMaintenanceDate { get; set; }
    public DateTime? NextMaintenanceDate { get; set; }
    public string? Owner { get; set; }
    public Dictionary<string, object>? Metadata { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public virtual ICollection<TelemetryLog> TelemetryLogs { get; set; } = new List<TelemetryLog>();
}

public enum AssetType
{
    CONTAINER = 0,
    VEHICLE = 1,
    MACHINERY = 2
}

public enum AssetStatus
{
    ACTIVE = 0,
    INACTIVE = 1,
    MAINTENANCE = 2,
    RETIRED = 3
}
