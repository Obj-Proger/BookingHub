using BookingHub.API.Common;
using BookingHub.Application.Features.Analytics.Queries.GetAnalyticsDashboard;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookingHub.API.Controllers.Analytics;

[Authorize]
[Route("api/v1/organizations/{organizationId:guid}/analytics")]
public sealed class AnalyticsController(IDispatcher dispatcher) : ApiControllerBase
{
    /// <param name="locationId">Omit for a network-wide report (Owner/Administrator only —
    /// a LocationManager scoped to one location cannot request this and will get 403).</param>
    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard(
        Guid organizationId, [FromQuery] Guid? locationId, [FromQuery] DateTime fromUtc, [FromQuery] DateTime toUtc,
        CancellationToken cancellationToken)
    {
        var result = await dispatcher.Send(
            new GetAnalyticsDashboardQuery(organizationId, locationId ?? Guid.Empty, AsUtc(fromUtc), AsUtc(toUtc)), cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Query-string binding turns an instant ending in "Z" into a Local-kind DateTime, which the database
    /// driver refuses to compare with timestamptz columns; converting restores the original UTC instant.
    /// A value without any offset is taken as already being UTC.
    /// </summary>
    private static DateTime AsUtc(DateTime value) =>
        value.Kind == DateTimeKind.Local ? value.ToUniversalTime() : DateTime.SpecifyKind(value, DateTimeKind.Utc);
}