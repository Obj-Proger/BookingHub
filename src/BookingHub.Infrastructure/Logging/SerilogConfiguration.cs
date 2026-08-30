using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Events;

namespace BookingHub.Infrastructure.Logging;

/// <summary>
/// Shared by both the bootstrap logger (built before the host exists, to capture startup
/// failures — Serilog's own recommended two-stage pattern for ASP.NET Core) and the final,
/// fully DI-aware logger built once configuration is available — API's Program.cs (not yet
/// written) uses this for both stages, so the two configurations can never drift apart.
/// </summary>
public static class SerilogConfiguration
{
    public static LoggerConfiguration Configure(LoggerConfiguration loggerConfiguration, IConfiguration configuration) =>
        loggerConfiguration
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
            .MinimumLevel.Override("Hangfire", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .Enrich.WithProperty("Application", "BookingHub")
            .WriteTo.Console()
            .WriteTo.Seq(configuration["Seq:ServerUrl"] ?? "http://localhost:5341");
}