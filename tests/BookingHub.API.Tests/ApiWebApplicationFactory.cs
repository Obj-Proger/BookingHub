using BookingHub.API.RateLimiting;
using BookingHub.API.Tests.TestDoubles;
using BookingHub.Infrastructure.Notifications;
using BookingHub.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace BookingHub.API.Tests;

public sealed class ApiWebApplicationFactory(string connectionString, bool relaxRateLimits = true) : WebApplicationFactory<Program>
{
    private static readonly string[] RelaxableLimits =
    [
        nameof(RateLimitingOptions.PublicReadPermitLimit),
        nameof(RateLimitingOptions.PublicWritePermitLimit),
        nameof(RateLimitingOptions.SignInPermitLimit),
        nameof(RateLimitingOptions.SessionPermitLimit),
        nameof(RateLimitingOptions.AuthenticatedPermitLimit)
    ];

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("ConnectionStrings:Database", connectionString);
        builder.UseSetting("Jwt:SigningKey", "test-only-signing-key-at-least-32-bytes-long");
        builder.UseSetting("Jwt:Issuer", "BookingHub.Tests");
        builder.UseSetting("Jwt:Audience", "BookingHub.Tests");

        // Default true — every consumer except RateLimitingTests wants a high ceiling so
        // unrelated functional flows (several logins/registrations in one test) never trip the
        // real limit by accident. RateLimitingTests explicitly passes false because verifying
        // the actual production default is the entire point of that test.
        if (relaxRateLimits)
        {
            foreach (var limit in RelaxableLimits)
                builder.UseSetting($"RateLimiting:{limit}", "10000");
        }

        // Guest messages go out within a fraction of a second, so tests that wait for one stay fast.
        builder.UseSetting("Outbox:PollingInterval", "00:00:00.2");

        // Several of these hosts run at once, each with its own Hangfire server: one worker apiece keeps
        // them well within the database server's connection limit.
        builder.UseSetting("Hangfire:WorkerCount", "1");

        builder.ConfigureServices(services =>
        {
            // First of all hosted services: the outbox processor and Hangfire query the schema as soon as they start.
            services.Insert(0, ServiceDescriptor.Singleton<IHostedService, DatabaseMigrator>());

            services.AddSingleton<GuestInbox>();
            services.RemoveAll<IEmailService>();
            services.AddScoped<IEmailService, RecordingEmailService>();
            services.RemoveAll<ISmsService>();
            services.AddScoped<ISmsService, RecordingSmsService>();
        });
    }

    /// <summary>Every message the API has sent to guests.</summary>
    public GuestInbox Inbox => Services.GetRequiredService<GuestInbox>();

    /// <summary>Brings the test database up to date when the host starts.</summary>
    private sealed class DatabaseMigrator(IServiceScopeFactory scopeFactory) : IHostedService
    {
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await using var scope = scopeFactory.CreateAsyncScope();
            await scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().Database.MigrateAsync(cancellationToken);
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}