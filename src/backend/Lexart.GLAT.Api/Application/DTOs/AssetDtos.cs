namespace Lexart.GLAT.Api.Application.DTOs;

public class AssetDto
{
    public Guid Id { get; set; }
    public string SerialNumber { get; set; } = null!;
    public string Type { get; set; } = null!;
    public string Status { get; set; } = null!;
    public string? Description { get; set; }
    public string Location { get; set; } = null!;
    public DateTime? LastMaintenanceDate { get; set; }
    public DateTime? NextMaintenanceDate { get; set; }
    public string? Owner { get; set; }
    public Dictionary<string, object>? Metadata { get; set; }
    public float? HealthScore { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CreateAssetRequest
{
    public string SerialNumber { get; set; } = null!;
    public string Type { get; set; } = null!;
    public string Status { get; set; } = "ACTIVE";
    public string? Description { get; set; }
    public string Location { get; set; } = null!;
    public DateTime? LastMaintenanceDate { get; set; }
    public DateTime? NextMaintenanceDate { get; set; }
    public string? Owner { get; set; }
    public Dictionary<string, object>? Metadata { get; set; }
}

public class UpdateAssetRequest
{
    public string? SerialNumber { get; set; }
    public string? Type { get; set; }
    public string? Status { get; set; }
    public string? Description { get; set; }
    public string? Location { get; set; }
    public DateTime? LastMaintenanceDate { get; set; }
    public DateTime? NextMaintenanceDate { get; set; }
    public string? Owner { get; set; }
    public Dictionary<string, object>? Metadata { get; set; }
}

public class AssetListResponse
{
    public List<AssetDto> Items { get; set; } = new();
    public int Total { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}
