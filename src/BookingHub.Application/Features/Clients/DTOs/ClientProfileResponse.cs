using BookingHub.Domain.Enums;

namespace BookingHub.Application.Features.Clients.DTOs;

public sealed record RevenueByCurrency(string Currency, decimal Amount);

public sealed record ClientVisitResponse(
    Guid BookingId, DateTime StartUtc, string ServiceName, string EmployeeFullName, BookingStatus Status,
    decimal PriceAmount, string PriceCurrency);

public sealed record ClientProfileResponse(
    Guid ClientId, string Phone, string? Name, string? Email,
    int TotalVisits, int CompletedVisits, int NoShowCount,
    IReadOnlyList<RevenueByCurrency> TotalRevenue, IReadOnlyList<RevenueByCurrency> AverageCheck,
    IReadOnlyList<ClientVisitResponse> Visits);