using BookingHub.Application.Common.Messaging;
using BookingHub.Application.Features.Bookings.DTOs;

namespace BookingHub.Application.Features.Bookings.Commands.CreateBooking;

/// <summary>Anonymous by design — the public self-service booking flow (Vision Document, §5.1).</summary>
public sealed record CreateBookingCommand(
    Guid OrganizationId, Guid LocationId, Guid EmployeeId, Guid ServiceId, DateTime StartUtc,
    string? Phone, string? ClientName, string? ClientEmail)
    : ICommand<BookingCreatedResponse>;