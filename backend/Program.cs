using dotnet_test.Data;
using dotnet_test.Services.Claude;
using dotnet_test.Services.Recommendations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// Aspire service defaults (OpenTelemetry, health checks, service discovery)
builder.AddServiceDefaults();

// Database — connection string injected by Aspire, fallback for running directly
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("dotnet-test-dev") ??
        builder.Configuration.GetConnectionString("DefaultConnection") ??
        "Host=localhost;Port=5432;Database=dotnet_test_dev;Username=postgres;Password=postgres"
    ));

// Controllers + Swagger
builder.Services.AddControllers();
builder.Services.AddScoped<IRecommendationEngine, RecommendationEngine>();
builder.Services.AddScoped<IRecommendationContentGenerator, RecommendationContentGenerator>();
builder.Services.Configure<ClaudeOptions>(builder.Configuration.GetSection(ClaudeOptions.SectionName));
builder.Services.AddHttpClient<IClaudeClient, ClaudeClient>((serviceProvider, httpClient) =>
{
    var options = serviceProvider.GetRequiredService<IOptions<ClaudeOptions>>().Value;
    httpClient.BaseAddress = new Uri(options.BaseUrl);
    httpClient.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
    httpClient.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// CORS
var corsOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
        policy.WithOrigins(corsOrigins).AllowAnyMethod().AllowAnyHeader());
});

var app = builder.Build();

// Run migrations and seed data on startup
await SeedData.InitializeAsync(app.Services);

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowFrontend");
app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
