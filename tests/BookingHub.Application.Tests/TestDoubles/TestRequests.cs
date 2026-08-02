using BookingHub.Application.Common.Security;

namespace BookingHub.Application.Tests.TestDoubles;

internal sealed record UnscopedTestRequest : IRequest<Result>;

internal sealed record OrgScopedTestRequest(Guid OrganizationId) : IRequest<Result>, IRequireOrganizationMembership;

internal sealed record OrgManagementTestRequest(Guid OrganizationId) : IRequest<Result>, IRequireOrganizationManagement;

internal sealed record LocationManagementTestRequest(Guid OrganizationId, Guid LocationId)
    : IRequest<Result>, IRequireLocationManagement;