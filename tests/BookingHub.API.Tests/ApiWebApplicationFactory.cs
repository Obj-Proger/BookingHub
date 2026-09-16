using BookingHub.Application.Common.Notifications;
using BookingHub.API.Tests.TestDoubles;
using BookingHub.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace BookingHub.API.Tests;

public sealed class ApiWebApplicationFactory(string connectionString, bool relaxRateLimits = true) : WebApplicationFactory<Program>
{
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
            builder.UseSetting("RateLimiting:PublicWritePermitLimit", "10000");

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IEmailService>();
            services.AddScoped<IEmailService, NoOpEmailService>();
            services.RemoveAll<ISmsService>();
            services.AddScoped<ISmsService, NoOpSmsService>();
        });
    }

    public async Task MigrateDatabaseAsync()
    {
        using var scope = Services.CreateScope();
        await scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().Database.MigrateAsync();
    }
}