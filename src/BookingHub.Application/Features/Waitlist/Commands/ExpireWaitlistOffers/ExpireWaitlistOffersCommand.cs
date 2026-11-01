using BookingHub.Application.Common.Messaging;
using BookingHub.Application.Common.Security;

namespace BookingHub.Application.Features.Waitlist.Commands.ExpireWaitlistOffers;

/// <summary>
/// Expires waitlist offers nobody accepted in time; sent by a background job. Each expiry offers
/// the slot to the next entry in line (see <c>WaitlistOfferExpiredHandler</c>).
/// </summary>
public sealed record ExpireWaitlistOffersCommand(Guid OrganizationId) : ICommand<int>, ISystemRequest;