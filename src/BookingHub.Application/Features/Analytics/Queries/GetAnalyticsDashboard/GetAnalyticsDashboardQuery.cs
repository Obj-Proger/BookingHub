using BookingHub.Application.Common.Messaging;
using BookingHub.Application.Common.Security;
using BookingHub.Application.Features.Analytics.DTOs;

namespace BookingHub.Application.Features.Analytics.Queries.GetAnalyticsDashboard;

/// <param name="LocationId">Guid.Empty means network-wide — safe as a sentinel because
/// Guard.NotEmpty (Domain) guarantees no real OrganizationMember ever has an empty LocationId,
/// so AuthorizationBehavior's existing LocationManager-match check already does the right thing
/// here without any special-casing.</param>
public sealed record GetAnalyticsDashboardQuery(Guid OrganizationId, Guid LocationId, DateTime FromUtc, DateTime ToUtc)
    : IQuery<AnalyticsDashboardResponse>, IRequireLocationManagement;