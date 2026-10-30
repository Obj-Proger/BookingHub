namespace BookingHub.Web.Api.Contracts;

public sealed record ClientSearchResultResponse(Guid ClientId, string Phone, string? Name);

public sealed record RevenueByCurrency(string Currency, decimal Amount);

public sealed record ClientVisitResponse(
    Guid BookingId, DateTime StartUtc, string LocationName, string LocationTimeZone, string ServiceName,
    string EmployeeFullName, BookingStatus Status, decimal? PriceAmount, string? PriceCurrency);

public sealed record ClientProfileResponse(
    Guid ClientId, string Phone, string? Name, string? Email,
    int TotalVisits, int CompletedVisits, int NoShowCount, bool FinancialsVisible,
    IReadOnlyList<RevenueByCurrency> TotalRevenue, IReadOnlyList<RevenueByCurrency> AverageCheck,
    IReadOnlyList<ClientVisitResponse> Visits);