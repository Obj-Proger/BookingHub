using BookingHub.Web.Api.Contracts;

namespace BookingHub.Web.Api;

public interface IPublicWaitlistApiClient
{
    Task<WaitlistEntryCreatedResponse?> JoinAsync(string organizationSlug, JoinWaitlistRequest request, CancellationToken cancellationToken);
    Task<bool> ConfirmOfferAsync(Guid waitlistEntryId, string? token, CancellationToken cancellationToken);
    Task<bool> LeaveAsync(Guid waitlistEntryId, string? token, CancellationToken cancellationToken);
}