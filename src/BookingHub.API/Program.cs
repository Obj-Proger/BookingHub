using BookingHub.API.BackgroundJobs;
using BookingHub.API.Common;
using BookingHub.Application;
using BookingHub.Infrastructure;
using BookingHub.Infrastructure.BackgroundJobs;
using BookingHub.Infrastructure.Logging;
using BookingHub.Infrastructure.Persistence;
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

    builder.Services.AddControllers();
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

    var app = builder.Build();

    app.UseSerilogRequestLogging();
    app.UseExceptionHandler();

    app.UseRouting();
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