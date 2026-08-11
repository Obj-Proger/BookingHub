using BookingHub.Application.Common.Messaging;
using BookingHub.Application.Features.Bookings.DTOs;

namespace BookingHub.Application.Features.Bookings.Commands.CreateRecurringBookingSeries;

/// <summary>Anonymous by design — same public path as CreateBookingCommand (Vision Document, §5.1).</summary>
/// <param name="IntervalWeeks">Weeks between occurrences, e.g. 2 for "every two weeks".</param>
/// <param name="OccurrenceCount">Total visits in the series, including the first.</param>
public sealed record CreateRecurringBookingSeriesCommand(
    Guid OrganizationId, Guid LocationId, Guid EmployeeId, Guid ServiceId, DateTime FirstStartUtc,
    int IntervalWeeks, int OccurrenceCount, string? Phone, string? ClientName, string? ClientEmail)
    : ICommand<RecurringBookingSeriesCreatedResponse>;