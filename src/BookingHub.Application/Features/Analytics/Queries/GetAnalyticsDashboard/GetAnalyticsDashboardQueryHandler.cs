using BookingHub.Application.Common;
using BookingHub.Application.Common.Messaging;
using BookingHub.Application.Common.Persistence;
using BookingHub.Application.Common.Security;
using BookingHub.Application.Features.Analytics.DTOs;
using BookingHub.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace BookingHub.Application.Features.Analytics.Queries.GetAnalyticsDashboard;

internal sealed class GetAnalyticsDashboardQueryHandler(
    IApplicationDbContext dbContext, ICurrentUser currentUser, IOrganizationMemberRepository organizationMemberRepository)
    : IQueryHandler<GetAnalyticsDashboardQuery, AnalyticsDashboardResponse>
{
    public async Task<Result<AnalyticsDashboardResponse>> Handle(GetAnalyticsDashboardQuery query, CancellationToken cancellationToken)
    {
        // AuthorizationBehavior already confirmed the caller is Owner/Administrator/matching
        // LocationManager. Re-fetched here (see GetClientProfileQueryHandler for the same
        // pattern) because the toggle check below needs the Role itself, not just a yes/no.
        var caller = await organizationMemberRepository.GetByOrganizationAndUserAsync(query.OrganizationId, currentUser.UserId, cancellationToken);
        if (caller is null)
            return Result.Failure<AnalyticsDashboardResponse>(ApplicationErrors.Authorization.NotAMember);

        if (caller.Role == OrganizationRole.Administrator)
        {
            var canViewFinancials = await dbContext.Organizations
                .Where(o => o.Id == query.OrganizationId)
                .Select(o => o.CanAdministratorsViewFinancials)
                .FirstAsync(cancellationToken);

            if (!canViewFinancials)
                return Result.Failure<AnalyticsDashboardResponse>(ApplicationErrors.Authorization.FinancialAccessDisabled);
        }

        var bookingsInPeriod = dbContext.Bookings
            .Where(b => b.OrganizationId == query.OrganizationId && b.TimeSlot.StartUtc >= query.FromUtc && b.TimeSlot.StartUtc < query.ToUtc);

        if (query.LocationId != Guid.Empty)
            bookingsInPeriod = bookingsInPeriod.Where(b => b.LocationId == query.LocationId);

        var completed = bookingsInPeriod.Where(b => b.Status == BookingStatus.Completed);

        var totalRevenue = await completed
            .GroupBy(b => b.Price.Currency)
            .Select(g => new RevenueByCurrency(g.Key, g.Sum(b => b.Price.Amount)))
            .ToListAsync(cancellationToken);

        var locationUtilization = await (
            from b in completed
            join l in dbContext.Locations on b.LocationId equals l.Id
            group b by new { l.Id, l.Name } into g
            select new LocationUtilizationResponse(g.Key.Id, g.Key.Name, g.Count()))
            .ToListAsync(cancellationToken);

        var employeeUtilization = await (
            from b in bookingsInPeriod
            where b.Status == BookingStatus.Completed || b.Status == BookingStatus.NoShow
            join e in dbContext.Employees on b.EmployeeId equals e.Id
            group b by new { e.Id, e.FullName } into g
            select new EmployeeUtilizationResponse(
                g.Key.Id, g.Key.FullName,
                g.Count(b => b.Status == BookingStatus.Completed),
                g.Count(b => b.Status == BookingStatus.NoShow)))
            .ToListAsync(cancellationToken);

        var popularServices = await (
            from b in completed
            join s in dbContext.Services on b.ServiceId equals s.Id
            group b by new { s.Id, s.Name } into g
            orderby g.Count() descending
            select new PopularServiceResponse(g.Key.Id, g.Key.Name, g.Count()))
            .Take(10)
            .ToListAsync(cancellationToken);

        var completedStarts = await (
            from b in completed
            join l in dbContext.Locations on b.LocationId equals l.Id
            select new { b.TimeSlot.StartUtc, l.TimeZone })
            .ToListAsync(cancellationToken);

        var peakHours = ToLocalHourHistogram(completedStarts.Select(b => (b.StartUtc, b.TimeZone)));

        return new AnalyticsDashboardResponse(totalRevenue, locationUtilization, employeeUtilization, popularServices, peakHours);
    }

    /// <summary>Counts bookings per hour of the day on the clock of the location each one took place at.</summary>
    private static List<PeakHourResponse> ToLocalHourHistogram(IEnumerable<(DateTime StartUtc, string TimeZone)> starts)
    {
        var zones = new Dictionary<string, TimeZoneInfo>();

        return
        [
            .. starts
                .Select(start =>
                {
                    if (!zones.TryGetValue(start.TimeZone, out var zone))
                        zones[start.TimeZone] = zone = TimeZoneInfo.FindSystemTimeZoneById(start.TimeZone);

                    return TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(start.StartUtc, DateTimeKind.Utc), zone).Hour;
                })
                .GroupBy(hour => hour)
                .Select(g => new PeakHourResponse(g.Key, g.Count()))
                .OrderBy(peak => peak.HourOfDay)
        ];
    }
}