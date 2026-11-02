using BookingHub.Web.Api.Contracts;

namespace BookingHub.Web.Api;

internal sealed class PublicWaitlistApiClient(HttpClient httpClient) : IPublicWaitlistApiClient
{
    public Task<ApiResult<WaitlistEntryCreatedResponse>> JoinAsync(
        string organizationSlug, JoinWaitlistRequest request, CancellationToken cancellationToken) =>
        httpClient.PostApiResultAsync<WaitlistEntryCreatedResponse>(
            $"api/v1/public/{organizationSlug}/waitlist", request, cancellationToken);

    public Task<ApiResult> ConfirmOfferAsync(string organizationSlug, Guid waitlistEntryId, string? token, CancellationToken cancellationToken) =>
        httpClient.PostApiResultAsync(
            $"api/v1/public/{organizationSlug}/waitlist/{waitlistEntryId}/confirm",
            new ConfirmWaitlistOfferRequest(token), cancellationToken);

    public Task<ApiResult> LeaveAsync(string organizationSlug, Guid waitlistEntryId, string? token, CancellationToken cancellationToken) =>
        httpClient.PostApiResultAsync(
            $"api/v1/public/{organizationSlug}/waitlist/{waitlistEntryId}/leave",
            new LeaveWaitlistRequest(token), cancellationToken);
}