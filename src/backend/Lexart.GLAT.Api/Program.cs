using Lexart.GLAT.Api.Application.Services;
using Lexart.GLAT.Api.Infrastructure.Data;
using Lexart.GLAT.Api.Infrastructure.Mapping;
using Lexart.GLAT.Api.Infrastructure.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console()
    .WriteTo.File("logs/glat-api-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo 
    { 
        Title = "GLAT API", 
        Version = "v1",
        Description = "Global Logistics Asset Tracker API"
    });

    // Add JWT auth definition
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        Description = "JWT Authorization header using the Bearer scheme."
    });

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] { }
        }
    });
});

// Configure Entity Framework Core
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? "Server=sqlserver;Database=glatdb;User Id=sa;Password=P@ssw0rd123!;Encrypt=False;";

builder.Services.AddDbContext<GlatDbContext>(options =>
    options.UseSqlServer(connectionString)
);

// Configure AutoMapper
builder.Services.AddAutoMapper(typeof(MappingProfile));

// Configure Authentication
var jwtSecret = builder.Configuration["Jwt:Secret"] ?? "your-super-secret-key-change-in-production-environment";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "glat-api";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "glat-clients";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,
            ValidateAudience = true,
            ValidAudience = jwtAudience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

// Register application services
builder.Services.AddScoped<IAssetService, AssetService>();
builder.Services.AddScoped<ITelemetryService, TelemetryService>();
builder.Services.AddScoped<IHealthScoreService, HealthScoreService>();
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();

// Add CORS if needed
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

var app = builder.Build();

// Migrate database on startup
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<GlatDbContext>();
    
    try
    {
        await dbContext.Database.EnsureCreatedAsync();
        // Initialize seed data
        await SeedInitialData(dbContext);
    }
    catch (Microsoft.Data.SqlClient.SqlException ex) when (ex.Number == 1801)
    {
        // Database already exists, just ensure schema is up-to-date
        Log.Warning("Database already exists, skipping creation");
        await SeedInitialData(dbContext);
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Error during database initialization");
        throw;
    }
}

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Enable Swagger for all environments (local development)
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "GLAT API V1");
    c.RoutePrefix = "swagger"; // Make it available at /swagger
});

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

await app.RunAsync();

// Seed initial data
static async Task SeedInitialData(GlatDbContext context)
{
    // Check if data already exists
    if (await context.Users.AnyAsync())
        return;

    var user = new Lexart.GLAT.Api.Domain.Entities.User
    {
        Id = Guid.NewGuid(),
        Username = "demo",
        Email = "demo@lexart.tech",
        PasswordHash = BCrypt.Net.BCrypt.HashPassword("demo123"),
        IsActive = true,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow
    };

    context.Users.Add(user);
    await context.SaveChangesAsync();
}

