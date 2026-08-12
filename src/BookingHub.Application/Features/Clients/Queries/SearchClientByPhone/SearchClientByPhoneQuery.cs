using BookingHub.Application.Common.Messaging;
using BookingHub.Application.Common.Security;
using BookingHub.Application.Features.Clients.DTOs;

namespace BookingHub.Application.Features.Clients.Queries.SearchClientByPhone;

public sealed record SearchClientByPhoneQuery(Guid OrganizationId, string? Phone)
    : IQuery<ClientSearchResultResponse>, IRequireOrganizationMembership;