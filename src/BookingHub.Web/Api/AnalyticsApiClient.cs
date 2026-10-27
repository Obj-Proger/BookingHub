using System.Globalization;
using BookingHub.Web.Api.Contracts;
using BookingHub.Web.Auth;

namespace BookingHub.Web.Api;

internal sealed class AnalyticsApiClient(AuthorizedApiClient authorizedClient) : IAnalyticsApiClient
{
    public Task<ApiResult<AnalyticsDashboardResponse>> GetDashboardAsync(
        Guid organizationId, Guid? locationId, DateTime fromUtc, DateTime toUtc, CancellationToken cancellationToken)
    {
        var query = $"fromUtc={Instant(fromUtc)}&toUtc={Instant(toUtc)}";
        if (locationId is { } id)
            query += $"&locationId={id}";

        return authorizedClient.GetJsonAsync<AnalyticsDashboardResponse>(
            $"api/v1/organizations/{organizationId}/analytics/dashboard?{query}", cancellationToken);
    }

    private static string Instant(DateTime utc) =>
        Uri.EscapeDataString(DateTime.SpecifyKind(utc, DateTimeKind.Utc).ToString("O", CultureInfo.InvariantCulture));
}