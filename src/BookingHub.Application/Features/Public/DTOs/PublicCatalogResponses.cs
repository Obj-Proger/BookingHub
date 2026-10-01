namespace BookingHub.Application.Features.Public.DTOs;

public sealed record PublicLocationResponse(Guid LocationId, string Name, string Address, string TimeZone);

public sealed record PublicServiceResponse(Guid ServiceId, string Name, TimeSpan Duration, decimal PriceAmount, string PriceCurrency);

public sealed record PublicEmployeeResponse(Guid EmployeeId, string FullName);