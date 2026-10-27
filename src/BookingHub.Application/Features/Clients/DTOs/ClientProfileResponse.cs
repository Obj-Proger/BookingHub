using BookingHub.Domain.Enums;

namespace BookingHub.Application.Features.Clients.DTOs;

public sealed record RevenueByCurrency(string Currency, decimal Amount);

/// <param name="LocationTimeZone">IANA id of the location the visit took place at; <paramref name="StartUtc"/> is local to it.</param>
public sealed record ClientVisitResponse(
    Guid BookingId, DateTime StartUtc, string LocationName, string LocationTimeZone, string ServiceName,
    string EmployeeFullName, BookingStatus Status, decimal PriceAmount, string PriceCurrency);

public sealed record ClientProfileResponse(
    Guid ClientId, string Phone, string? Name, string? Email,
    int TotalVisits, int CompletedVisits, int NoShowCount,
    IReadOnlyList<RevenueByCurrency> TotalRevenue, IReadOnlyList<RevenueByCurrency> AverageCheck,
    IReadOnlyList<ClientVisitResponse> Visits);