using BookingHub.Application.Common.Persistence;
using BookingHub.Infrastructure.Persistence;
using BookingHub.Infrastructure.Persistence.Interceptors;
using BookingHub.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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

        return services;
    }
}