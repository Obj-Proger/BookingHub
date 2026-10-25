using BookingHub.Application.Common.Messaging;
using BookingHub.Application.Common.Security;
using BookingHub.Application.Features.Services.DTOs;

namespace BookingHub.Application.Features.Services.Queries.GetServices;

public sealed record GetServicesQuery(Guid OrganizationId)
    : IQuery<IReadOnlyList<ServiceResponse>>, IRequireOrganizationMembership;