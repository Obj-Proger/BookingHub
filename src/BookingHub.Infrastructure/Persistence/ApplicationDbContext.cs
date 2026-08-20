using BookingHub.Application.Common.Persistence;
using BookingHub.Domain.Entities;
using BookingHub.Domain.Enums;
using BookingHub.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BookingHub.Infrastructure.Persistence;

public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityUserContext<ApplicationUser, Guid>(options), IApplicationDbContext
{
    public DbSet<Organization> Organizations => Set<Organization>();
    public DbSet<OrganizationMember> OrganizationMembers => Set<OrganizationMember>();
    public DbSet<Location> Locations => Set<Location>();
    public DbSet<Client> Clients => Set<Client>();
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<EmployeeLocationAssignment> EmployeeLocationAssignments => Set<EmployeeLocationAssignment>();
    public DbSet<RecurringSchedule> RecurringSchedules => Set<RecurringSchedule>();
    public DbSet<ScheduleException> ScheduleExceptions => Set<ScheduleException>();
    public DbSet<Service> Services => Set<Service>();
    public DbSet<LocationServiceOverride> LocationServiceOverrides => Set<LocationServiceOverride>();
    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<WaitlistEntry> WaitlistEntries => Set<WaitlistEntry>();
    public DbSet<Review> Reviews => Set<Review>();

    // DbSet<T> implements IQueryable<T>, but a property declared as DbSet<T> does not
    // automatically satisfy an interface member declared as IQueryable<T> — the two are
    // different declared types as far as implicit interface implementation is concerned.
    // Explicit implementation bridges the two without exposing EF-specific DbSet members
    // (Add/Remove/Attach) through the Application-facing port.
    IQueryable<Organization> IApplicationDbContext.Organizations => Organizations;
    IQueryable<OrganizationMember> IApplicationDbContext.OrganizationMembers => OrganizationMembers;
    IQueryable<Location> IApplicationDbContext.Locations => Locations;
    IQueryable<Employee> IApplicationDbContext.Employees => Employees;
    IQueryable<EmployeeLocationAssignment> IApplicationDbContext.EmployeeLocationAssignments => EmployeeLocationAssignments;
    IQueryable<RecurringSchedule> IApplicationDbContext.RecurringSchedules => RecurringSchedules;
    IQueryable<ScheduleException> IApplicationDbContext.ScheduleExceptions => ScheduleExceptions;
    IQueryable<Service> IApplicationDbContext.Services => Services;
    IQueryable<LocationServiceOverride> IApplicationDbContext.LocationServiceOverrides => LocationServiceOverrides;
    IQueryable<Booking> IApplicationDbContext.Bookings => Bookings;
    IQueryable<Review> IApplicationDbContext.Reviews => Reviews;

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        base.ConfigureConventions(configurationBuilder);

        // One line per enum type in the model, not per property that uses it — HaveConversion<string>
        // applies model-wide to every property of that CLR enum type, wherever it appears.
        configurationBuilder.Properties<OrganizationRole>().HaveConversion<string>();
        configurationBuilder.Properties<BookingStatus>().HaveConversion<string>();
        configurationBuilder.Properties<BookingSource>().HaveConversion<string>();
        configurationBuilder.Properties<ScheduleExceptionType>().HaveConversion<string>();
        configurationBuilder.Properties<WaitlistEntryStatus>().HaveConversion<string>();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}