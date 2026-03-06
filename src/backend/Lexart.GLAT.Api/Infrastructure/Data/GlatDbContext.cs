using Lexart.GLAT.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Lexart.GLAT.Api.Infrastructure.Data;

public class GlatDbContext : DbContext
{
    public GlatDbContext(DbContextOptions<GlatDbContext> options) : base(options)
    {
    }

    public virtual DbSet<Asset> Assets { get; set; } = null!;
    public virtual DbSet<TelemetryLog> TelemetryLogs { get; set; } = null!;
    public virtual DbSet<User> Users { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Asset>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.SerialNumber)
                .IsRequired()
                .HasMaxLength(100);
            entity.Property(e => e.Type)
                .IsRequired();
            entity.Property(e => e.Status)
                .IsRequired();
            entity.Property(e => e.Location)
                .IsRequired()
                .HasMaxLength(200);
            entity.Property(e => e.Owner)
                .HasMaxLength(100);
            entity.Property(e => e.Description)
                .HasMaxLength(500);

            entity.HasIndex(e => e.SerialNumber)
                .IsUnique();

            entity.HasIndex(e => e.Status);

            entity.HasMany(e => e.TelemetryLogs)
                .WithOne(t => t.Asset)
                .HasForeignKey(t => t.AssetId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.Property(e => e.Metadata)
                .HasConversion(
                    v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                    v => System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(v, (System.Text.Json.JsonSerializerOptions?)null)
                );
        });

        modelBuilder.Entity<TelemetryLog>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.AssetId)
                .IsRequired();
            entity.Property(e => e.Timestamp)
                .IsRequired();
            entity.Property(e => e.CreatedAt)
                .IsRequired();

            entity.OwnsOne(e => e.SensorStatus, sb =>
            {
                sb.Property(s => s.Temperature).IsRequired();
                sb.Property(s => s.Humidity).IsRequired();
                sb.Property(s => s.Pressure).IsRequired();
                sb.Property(s => s.Vibration).IsRequired();
                sb.OwnsOne(s => s.Gps);
            });

            entity.Property(e => e.Alerts)
                .HasConversion(
                    v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                    v => System.Text.Json.JsonSerializer.Deserialize<List<Alert>>(v, (System.Text.Json.JsonSerializerOptions?)null)
                );

            entity.HasIndex(e => e.AssetId);
            entity.HasIndex(e => e.Timestamp);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Username)
                .IsRequired()
                .HasMaxLength(100);
            entity.Property(e => e.Email)
                .IsRequired()
                .HasMaxLength(200);
            entity.Property(e => e.PasswordHash)
                .IsRequired();

            entity.HasIndex(e => e.Username)
                .IsUnique();
        });
    }
}
