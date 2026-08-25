using BookingHub.Application.Common.Persistence;
using BookingHub.Domain.Entities;
using BookingHub.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace BookingHub.Infrastructure.Persistence.Repositories;

internal sealed class BookingRepository(ApplicationDbContext dbContext) : IBookingRepository
{
    public void Add(Booking booking) => dbContext.Bookings.Add(booking);

    public Task<Booking?> GetByIdAsync(Guid bookingId, CancellationToken cancellationToken) =>
        dbContext.Bookings.FirstOrDefaultAsync(b => b.Id == bookingId, cancellationToken);

    public Task<Booking?> GetByIdAsync(
        Guid organizationId, Guid locationId, Guid employeeId, Guid bookingId, CancellationToken cancellationToken) =>
        dbContext.Bookings.FirstOrDefaultAsync(
            b => b.Id == bookingId && b.OrganizationId == organizationId && b.LocationId == locationId && b.EmployeeId == employeeId,
            cancellationToken);

    public async Task<IReadOnlyList<Booking>> GetPendingBookingsPastConfirmationWindowAsync(DateTime utcNow, CancellationToken cancellationToken) =>
        await dbContext.Bookings
            .Where(b => b.Status == BookingStatus.Pending)
            .Join(dbContext.Organizations, b => b.OrganizationId, o => o.Id, (b, o) => new { Booking = b, o.PendingConfirmationWindow })
            .Where(x => x.Booking.CreatedAtUtc + x.PendingConfirmationWindow < utcNow)
            .Select(x => x.Booking)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Booking>> GetConfirmedBookingsWithEndedSlotsAsync(DateTime utcNow, CancellationToken cancellationToken) =>
        await dbContext.Bookings
            .Where(b => b.Status == BookingStatus.Confirmed && b.TimeSlot.EndUtc <= utcNow)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Booking>> GetAwaitingReviewBookingsPastAutoCompleteWindowAsync(DateTime utcNow, CancellationToken cancellationToken) =>
        await dbContext.Bookings
            .Where(b => b.Status == BookingStatus.AwaitingReview)
            .Join(dbContext.Organizations, b => b.OrganizationId, o => o.Id, (b, o) => new { Booking = b, o.AutoCompleteWindow })
            .Where(x => x.Booking.TimeSlot.EndUtc + x.AutoCompleteWindow < utcNow)
            .Select(x => x.Booking)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Booking>> GetPendingSiblingsInSeriesAsync(
        Guid recurringSeriesId, Guid excludingBookingId, CancellationToken cancellationToken) =>
        await dbContext.Bookings
            .Where(b => b.RecurringSeriesId == recurringSeriesId && b.Id != excludingBookingId && b.Status == BookingStatus.Pending)
            .ToListAsync(cancellationToken);
}