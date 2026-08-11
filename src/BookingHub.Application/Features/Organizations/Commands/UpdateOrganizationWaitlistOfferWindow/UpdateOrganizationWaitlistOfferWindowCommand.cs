using BookingHub.Application.Common.Security;
using BookingHub.Application.Common.Messaging;

namespace BookingHub.Application.Features.Organizations.Commands.UpdateOrganizationWaitlistOfferWindow;

public sealed record UpdateOrganizationWaitlistOfferWindowCommand(Guid OrganizationId, TimeSpan Window)
    : ICommand, IRequireOrganizationManagement;