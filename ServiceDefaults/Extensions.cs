using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

namespace Microsoft.Extensions.Hosting;

/// <summary>
/// Extension methods for Aspire service defaults.
/// </summary>
public static class AspireExtensions
{
    /// <summary>
    /// Add Aspire service defaults to the host builder.
    /// </summary>
    public static IHostApplicationBuilder AddServiceDefaults(this IHostApplicationBuilder builder)
    {
        builder.ConfigureOpenTelemetry();
        builder.AddDefaultHealthChecks();
        builder.Services.AddServiceDiscovery();
        builder.Services.ConfigureHttpClientDefaults(http =>
        {
            http.AddStandardResilienceHandler();
        });

        return builder;
    }

    private static void ConfigureOpenTelemetry(this IHostApplicationBuilder builder)
    {
        builder.Logging.AddOpenTelemetry(logging =>
        {
            logging.ConsoleExporter.IncludeFormattedMessage = true;
        });

        builder.Metrics.AddMeter("Microsoft.AspNetCore.Hosting", "System.Net.Http");

        builder.Tracing
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddConsoleExporter();
    }

    private static void AddDefaultHealthChecks(this IHostApplicationBuilder builder)
    {
        builder.Services
            .AddHealthChecks()
            .AddCheck("self", () => HealthCheckResult.Healthy(), ["live"]);
    }
}
