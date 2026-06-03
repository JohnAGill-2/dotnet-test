using dotnet_test.Data;
using dotnet_test.Services.Claude;
using dotnet_test.Services.Auth;
using dotnet_test.Services.Recommendations;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Diagnostics;
using System.Text;

// Configure Serilog for structured logging
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console(outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz}] [{Level:u3}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

try
{
    // Ensure the Postgres Docker container is running before anything else
    await EnsurePostgresContainerAsync();

    var builder = WebApplication.CreateBuilder(args);
    builder.Host.UseSerilog();

    // Add Aspire service defaults
    builder.AddServiceDefaults();

// Database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection") ?? 
        "Host=localhost;Port=5432;Database=dotnet_test;Username=postgres;Password=postgres"));

// Controllers
builder.Services.AddControllers();
builder.Services.AddScoped<IRecommendationEngine, RecommendationEngine>();
builder.Services.AddScoped<IRecommendationContentGenerator, RecommendationContentGenerator>();
builder.Services.AddScoped<IRiskRecalculationService, RiskRecalculationService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.Configure<ClaudeOptions>(builder.Configuration.GetSection(ClaudeOptions.SectionName));
builder.Services.Configure<RiskRecalculationOptions>(builder.Configuration.GetSection(RiskRecalculationOptions.SectionName));
builder.Services.Configure<AuthOptions>(builder.Configuration.GetSection(AuthOptions.SectionName));

var authOptions = builder.Configuration.GetSection(AuthOptions.SectionName).Get<AuthOptions>() ?? new AuthOptions();
var signingKey = Encoding.UTF8.GetBytes(authOptions.SigningKey);

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ValidIssuer = authOptions.Issuer,
            ValidAudience = authOptions.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(signingKey),
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddHttpClient<IClaudeClient, ClaudeClient>((serviceProvider, httpClient) =>
{
    var options = serviceProvider.GetRequiredService<IOptions<ClaudeOptions>>().Value;

    httpClient.BaseAddress = new Uri(options.BaseUrl);
    httpClient.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
    httpClient.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");
});

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// CORS
var corsOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policyBuilder =>
    {
        policyBuilder
            .WithOrigins(corsOrigins)
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

var app = builder.Build();

// Seed database
await SeedData.InitializeAsync(app.Services);

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

// Global error handling middleware
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;

        var exceptionHandlerPathFeature = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerPathFeature>();
        var exception = exceptionHandlerPathFeature?.Error;

        logger.LogError(exception, "Unhandled exception occurred: {Message}", exception?.Message);

        await context.Response.WriteAsJsonAsync(new
        {
            error = exception?.Message ?? "An unexpected error occurred",
            timestamp = DateTime.UtcNow,
            requestId = context.TraceIdentifier
        });
    });
});

app.UseCors("AllowFrontend");

app.MapControllers();

app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

static async Task EnsurePostgresContainerAsync()
{
    const string containerName = "dotnet-test-db";

    // Check if already running
    var running = await RunDockerAsync($"ps --filter name={containerName} --filter status=running -q");
    if (!string.IsNullOrWhiteSpace(running)) return;

    // Check if container exists but is stopped
    var exists = await RunDockerAsync($"ps -a --filter name={containerName} -q");
    if (!string.IsNullOrWhiteSpace(exists))
    {
        Console.WriteLine($"[docker] Starting existing container '{containerName}'...");
        await RunDockerAsync($"start {containerName}");
    }
    else
    {
        Console.WriteLine($"[docker] Creating and starting container '{containerName}'...");
        await RunDockerAsync(
            $"run -d --name {containerName} " +
            "-e POSTGRES_USER=postgres " +
            "-e POSTGRES_PASSWORD=postgres " +
            "-e POSTGRES_DB=dotnet_test_dev " +
            "-p 5432:5432 " +
            "postgres:17");
    }

    // Wait for Postgres to be ready (up to 15s)
    Console.WriteLine("[docker] Waiting for Postgres to be ready...");
    for (var i = 0; i < 15; i++)
    {
        var ready = await RunDockerAsync($"exec {containerName} pg_isready -U postgres");
        if (ready.Contains("accepting connections")) { Console.WriteLine("[docker] Postgres is ready."); return; }
        await Task.Delay(1000);
    }
    Console.WriteLine("[docker] Warning: Postgres may not be ready yet — proceeding anyway.");
}

static async Task<string> RunDockerAsync(string arguments)
{
    using var process = new Process
    {
        StartInfo = new ProcessStartInfo("docker", arguments)
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        }
    };
    process.Start();
    var output = await process.StandardOutput.ReadToEndAsync();
    await process.WaitForExitAsync();
    return output.Trim();
}
