using BookingHub.Application.Common.Messaging;
using BookingHub.Application.Features.Waitlist.DTOs;

namespace BookingHub.Application.Features.Waitlist.Commands.JoinWaitlist;

/// <summary>
/// Anonymous by design — same public path as <c>CreateBookingCommand</c> (Vision Document, §5.4).
/// </summary>
public sealed record JoinWaitlistCommand(
    Guid OrganizationId, Guid LocationId, Guid? EmployeeId, Guid ServiceId,
    DateTime DesiredStartUtc, DateTime DesiredEndUtc, string? Phone, string? ClientName, string? ClientEmail)
    : ICommand<WaitlistEntryCreatedResponse>;