using BookingHub.Web.Api.Contracts;

namespace BookingHub.Web.Api;

public interface IPublicWaitlistApiClient
{
    Task<ApiResult<WaitlistEntryCreatedResponse>> JoinAsync(
        string organizationSlug, JoinWaitlistRequest request, CancellationToken cancellationToken);

    Task<ApiResult> ConfirmOfferAsync(string organizationSlug, Guid waitlistEntryId, string? token, CancellationToken cancellationToken);

    Task<ApiResult> LeaveAsync(string organizationSlug, Guid waitlistEntryId, string? token, CancellationToken cancellationToken);
}