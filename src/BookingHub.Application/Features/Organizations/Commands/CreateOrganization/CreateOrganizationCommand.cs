using BookingHub.Application.Common.Messaging;
using BookingHub.Application.Common.Security;
using BookingHub.Application.Features.Organizations.DTOs;

namespace BookingHub.Application.Features.Organizations.Commands.CreateOrganization;

/// <summary>Any signed-in user may create an organization; the caller becomes its Owner.</summary>
public sealed record CreateOrganizationCommand(string? Name, string? Slug) : ICommand<OrganizationCreatedResponse>, IAuthenticatedRequest;