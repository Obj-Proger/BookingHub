using BookingHub.Application.Common.Notifications;
using BookingHub.Application.Common.Persistence;
using BookingHub.Application.Common.Security;
using BookingHub.Infrastructure.BackgroundJobs;
using BookingHub.Infrastructure.Identity;
using BookingHub.Infrastructure.Notifications;
using BookingHub.Infrastructure.Persistence;
using BookingHub.Infrastructure.Persistence.Interceptors;
using BookingHub.Infrastructure.Persistence.Repositories;
using BookingHub.Infrastructure.Web;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Hangfire;
using Hangfire.PostgreSql;
using System.Text;

namespace BookingHub.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<AuditableEntitySaveChangesInterceptor>();
        services.AddScoped<DomainEventDispatchingSaveChangesInterceptor>();

        services.AddDbContext<ApplicationDbContext>((sp, options) =>
            options.UseNpgsql(configuration.GetConnectionString("Database"))
                .AddInterceptors(
                    sp.GetRequiredService<AuditableEntitySaveChangesInterceptor>(),
                    sp.GetRequiredService<DomainEventDispatchingSaveChangesInterceptor>()));

        services.AddIdentityCore<ApplicationUser>(options =>
        {
            options.Password.RequiredLength = 8;
            options.Password.RequireNonAlphanumeric = false;
            options.User.RequireUniqueEmail = true;
        })
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders();

        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();

        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, CurrentUser>();
        services.AddScoped<TenantContext>();
        services.AddScoped<ICurrentTenant>(sp => sp.GetRequiredService<TenantContext>());
        services.AddScoped<RowLevelSecuritySession>();
        services.AddScoped<ITenantScope>(sp => sp.GetRequiredService<RowLevelSecuritySession>());

        var jwtOptions = configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()
            ?? throw new InvalidOperationException("The \"Jwt\" configuration section is missing.");

        // The configuration binder leaves SigningKey null when the setting is absent, 'required'
        // notwithstanding. HS256 needs a key of at least 256 bits; anything shorter would fail only
        // on the first token operation (IDX10703/IDX10720), so it is rejected here, at startup.
        var signingKey = Encoding.UTF8.GetBytes(jwtOptions.SigningKey ?? string.Empty);
        if (signingKey.Length < JwtOptions.MinimumSigningKeyBytes)
        {
            throw new InvalidOperationException(
                $"Jwt:SigningKey must be at least {JwtOptions.MinimumSigningKeyBytes} bytes long. " +
                "Set it with dotnet user-secrets locally (see README) or the Jwt__SigningKey environment variable when deployed.");
        }

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.MapInboundClaims = false; // keep "sub" as "sub" — see CurrentUser's remark
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = jwtOptions.Issuer,
                ValidateAudience = true,
                ValidAudience = jwtOptions.Audience,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(signingKey)
            };
        });

        services.AddNotifications(configuration);

        services.AddOptions<WebAppOptions>()
            .Bind(configuration.GetSection(WebAppOptions.SectionName))
            .Validate(
                options => Uri.TryCreate(options.BaseUrl, UriKind.Absolute, out _),
                "WebApp:BaseUrl must be the absolute URL of BookingHub.Web, e.g. https://localhost:7201 — links in notifications are built from it.")
            .ValidateOnStart();
        services.AddSingleton<IWebLinkBuilder, WebLinkBuilder>();

        services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddScoped<IOrganizationRepository, OrganizationRepository>();
        services.AddScoped<IOrganizationMemberRepository, OrganizationMemberRepository>();
        services.AddScoped<ILocationRepository, LocationRepository>();
        services.AddScoped<IClientRepository, ClientRepository>();
        services.AddScoped<IEmployeeRepository, EmployeeRepository>();
        services.AddScoped<IEmployeeLocationAssignmentRepository, EmployeeLocationAssignmentRepository>();
        services.AddScoped<IRecurringScheduleRepository, RecurringScheduleRepository>();
        services.AddScoped<IScheduleExceptionRepository, ScheduleExceptionRepository>();
        services.AddScoped<IServiceRepository, ServiceRepository>();
        services.AddScoped<ILocationServiceOverrideRepository, LocationServiceOverrideRepository>();
        services.AddScoped<IBookingRepository, BookingRepository>();
        services.AddScoped<IWaitlistEntryRepository, WaitlistEntryRepository>();
        services.AddScoped<IReviewRepository, ReviewRepository>();
        services.AddScoped<IRefreshTokenService, RefreshTokenService>();
        services.AddScoped<IUserDirectory, UserDirectory>();

        services.AddHangfire(config => config
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UsePostgreSqlStorage(options => options.UseNpgsqlConnection(configuration.GetConnectionString("Database"))));

        services.AddHangfireServer();
        services.AddSingleton<TenantJobRunner>();
        services.AddScoped<BookingLifecycleJobs>();

        return services;
    }
}