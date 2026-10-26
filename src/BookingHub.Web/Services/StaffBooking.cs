using BookingHub.Web.Api.Contracts;

namespace BookingHub.Web.Services;

/// <summary>A booking in the day view, whichever query it came from.</summary>
/// <param name="EmployeeName">Null when the day is the signed-in employee's own, where the name is redundant.</param>
public sealed record StaffBooking(
    Guid BookingId, Guid EmployeeId, string? EmployeeName, DateTime StartUtc, DateTime EndUtc,
    string ServiceName, string? ClientName, string ClientPhone, BookingStatus Status);

public static class StaffBookings
{
    public static StaffBooking From(LocationBookingResponse booking) => new(
        booking.BookingId, booking.EmployeeId, booking.EmployeeName, booking.StartUtc, booking.EndUtc,
        booking.ServiceName, booking.ClientName, booking.ClientPhone, booking.Status);

    public static StaffBooking From(EmployeeBookingResponse booking, Guid employeeId) => new(
        booking.BookingId, employeeId, null, booking.StartUtc, booking.EndUtc,
        booking.ServiceName, booking.ClientName, booking.ClientPhone, booking.Status);
}