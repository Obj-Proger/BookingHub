namespace BookingHub.Web.Api.Contracts;

public sealed record EmployeeBookingResponse(
    Guid BookingId, DateTime StartUtc, DateTime EndUtc, string ServiceName,
    string? ClientName, string ClientPhone, BookingStatus Status);

public sealed record LocationBookingResponse(
    Guid BookingId, Guid EmployeeId, string EmployeeName, DateTime StartUtc, DateTime EndUtc,
    string ServiceName, string? ClientName, string ClientPhone, BookingStatus Status);

public sealed record EmployeeLocationResponse(Guid LocationId, string LocationName);