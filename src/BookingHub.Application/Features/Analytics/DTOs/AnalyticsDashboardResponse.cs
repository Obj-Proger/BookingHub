namespace BookingHub.Application.Features.Analytics.DTOs;

public sealed record RevenueByCurrency(string Currency, decimal Amount);
public sealed record LocationUtilizationResponse(Guid LocationId, string LocationName, int CompletedBookings);
public sealed record EmployeeUtilizationResponse(Guid EmployeeId, string EmployeeFullName, int CompletedBookings, int NoShowCount);
public sealed record PopularServiceResponse(Guid ServiceId, string ServiceName, int BookingCount);
public sealed record PeakHourResponse(int HourOfDay, int BookingCount);

public sealed record AnalyticsDashboardResponse(
    IReadOnlyList<RevenueByCurrency> TotalRevenue,
    IReadOnlyList<LocationUtilizationResponse> LocationUtilization,
    IReadOnlyList<EmployeeUtilizationResponse> EmployeeUtilization,
    IReadOnlyList<PopularServiceResponse> PopularServices,
    IReadOnlyList<PeakHourResponse> PeakHours);