using BookingHub.API.BackgroundJobs;
using BookingHub.API.Common;
using BookingHub.Application;
using BookingHub.Infrastructure;
using BookingHub.Infrastructure.BackgroundJobs;
using BookingHub.Infrastructure.Identity;
using BookingHub.Infrastructure.Logging;
using BookingHub.Infrastructure.Persistence;
using Microsoft.AspNetCore.HttpOverrides;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using Hangfire;
using Scalar.AspNetCore;
using Serilog;

Log.Logger = SerilogConfiguration.Configure(new LoggerConfiguration(), new ConfigurationBuilder().Build())
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting BookingHub API");

    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, configuration) => SerilogConfiguration.Configure(configuration, context.Configuration));

    builder.Services.AddControllers()
        .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
    builder.Services.AddApplication();
    builder.Services.AddInfrastructure(builder.Configuration);
    builder.Services.AddOpenApi();
    builder.Services.AddHealthChecks().AddDbContextCheck<ApplicationDbContext>();

    builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
    builder.Services.AddProblemDetails();

    builder.Services.AddCors(options =>
    {
        var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];

        options.AddPolicy("Default", policy =>
            policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod());
    });

    builder.Services.Configure<ForwardedHeadersOptions>(options =>
    {
        options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
        // KnownProxies/KnownNetworks intentionally left at their defaults (trust none) here —
        // a real deployment must add its actual reverse proxy's address explicitly; trusting
        // X-Forwarded-For from an unconfigured wildcard would let any client spoof its own IP
        // and trivially bypass the per-IP limits below.
    });

    builder.Services.AddRateLimiter(options =>
    {
        options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

        options.AddPolicy("public-read", context => RateLimitPartition.GetSlidingWindowLimiter(
            GetPartitionKey(context),
            _ => new SlidingWindowRateLimiterOptions
            {
                Window = TimeSpan.FromMinutes(1),
                SegmentsPerWindow = 4,
                PermitLimit = 60,
                QueueLimit = 0
            }));

        options.AddPolicy("public-write", context => RateLimitPartition.GetSlidingWindowLimiter(
            GetPartitionKey(context),
            _ => new SlidingWindowRateLimiterOptions
            {
                Window = TimeSpan.FromMinutes(1),
                SegmentsPerWindow = 4,
                PermitLimit = 10,
                QueueLimit = 0
            }));

        static string GetPartitionKey(HttpContext context) => context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    });

    var app = builder.Build();

    app.UseForwardedHeaders();

    app.UseSerilogRequestLogging();
    app.UseExceptionHandler();

    app.UseRouting();
    app.UseRateLimiter();
    app.UseCors("Default");
    app.UseMiddleware<TenantResolutionMiddleware>();
    app.UseAuthentication();
    app.UseAuthorization();

    app.UseHangfireDashboard(options: new DashboardOptions
    {
        Authorization = [new HangfireDashboardAuthorizationFilter(app.Environment)]
    });
    HangfireJobScheduler.ScheduleRecurringJobs();

    app.MapControllers();
    app.MapHealthChecks("/health");

    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
        app.MapScalarApiReference();
    }

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "BookingHub API terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}