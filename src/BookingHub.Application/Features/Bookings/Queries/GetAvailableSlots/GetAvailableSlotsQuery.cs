using BookingHub.Application.Common.Messaging;
using BookingHub.Application.Features.Bookings.DTOs;

namespace BookingHub.Application.Features.Bookings.Queries.GetAvailableSlots;

/// <summary>Anonymous by design — this powers the public booking page (Vision Document, §5.1).</summary>
public sealed record GetAvailableSlotsQuery(Guid OrganizationId, Guid LocationId, Guid EmployeeId, Guid ServiceId, DateOnly Date)
    : IQuery<IReadOnlyList<AvailableSlotResponse>>;