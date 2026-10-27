using BookingHub.Web.Api.Contracts;

namespace BookingHub.Web.Api;

public interface IAnalyticsApiClient
{
    /// <param name="locationId">Null requests the network-wide report (Owner and Administrator only).</param>
    /// <param name="fromUtc">Inclusive start of the period.</param>
    /// <param name="toUtc">Exclusive end of the period.</param>
    Task<ApiResult<AnalyticsDashboardResponse>> GetDashboardAsync(
        Guid organizationId, Guid? locationId, DateTime fromUtc, DateTime toUtc, CancellationToken cancellationToken);
}