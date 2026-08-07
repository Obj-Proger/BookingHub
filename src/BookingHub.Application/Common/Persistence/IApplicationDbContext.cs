using BookingHub.Domain.Entities;

namespace BookingHub.Application.Common.Persistence;

/// <summary>
/// Read-only query access to persisted data for the Query side of CQRS — handlers project
/// directly into DTOs via LINQ, bypassing the Domain layer's behavior methods entirely.
/// Grows by one property per entity as query features need it, not all at once upfront.
/// </summary>
public interface IApplicationDbContext
{
    IQueryable<Organization> Organizations { get; }
    IQueryable<Location> Locations { get; }
    IQueryable<Employee> Employees { get; }
    IQueryable<Service> Services { get; }
    IQueryable<EmployeeLocationAssignment> EmployeeLocationAssignments { get; }
    IQueryable<RecurringSchedule> RecurringSchedules { get; }
    IQueryable<ScheduleException> ScheduleExceptions { get; }
    IQueryable<Booking> Bookings { get; }
    IQueryable<Review> Reviews { get; }
}