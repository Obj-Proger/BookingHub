using BookingHub.Application.Common.Messaging;
using BookingHub.Application.Common.Security;
using BookingHub.Application.Features.Services.DTOs;

namespace BookingHub.Application.Features.Services.Queries.GetService;

public sealed record GetServiceQuery(Guid OrganizationId, Guid ServiceId) : IQuery<ServiceResponse>, IRequireOrganizationMembership;