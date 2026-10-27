namespace BookingHub.Web.Api.Contracts;

public sealed record LocationUtilizationResponse(Guid LocationId, string LocationName, int CompletedBookings);

public sealed record EmployeeUtilizationResponse(Guid EmployeeId, string EmployeeFullName, int CompletedBookings, int NoShowCount);

public sealed record PopularServiceResponse(Guid ServiceId, string ServiceName, int BookingCount);

/// <param name="HourOfDay">0–23 on the clock of the location where the booking took place.</param>
public sealed record PeakHourResponse(int HourOfDay, int BookingCount);

public sealed record AnalyticsDashboardResponse(
    IReadOnlyList<RevenueByCurrency> TotalRevenue,
    IReadOnlyList<LocationUtilizationResponse> LocationUtilization,
    IReadOnlyList<EmployeeUtilizationResponse> EmployeeUtilization,
    IReadOnlyList<PopularServiceResponse> PopularServices,
    IReadOnlyList<PeakHourResponse> PeakHours);