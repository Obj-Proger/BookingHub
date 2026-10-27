using BookingHub.Web.Api.Contracts;
using BookingHub.Web.Auth;

namespace BookingHub.Web.Api;

internal sealed class StaffReviewsApiClient(AuthorizedApiClient authorizedClient) : IStaffReviewsApiClient
{
    private static string BasePath(Guid organizationId, Guid locationId) =>
        $"api/v1/organizations/{organizationId}/locations/{locationId}/reviews";

    public Task<ApiResult<IReadOnlyList<ReviewModerationResponse>>> GetLocationReviewsAsync(
        Guid organizationId, Guid locationId, int skip, int take, CancellationToken cancellationToken) =>
        authorizedClient.GetJsonAsync<IReadOnlyList<ReviewModerationResponse>>(
            $"{BasePath(organizationId, locationId)}?skip={skip}&take={take}", cancellationToken);

    public Task<ApiResult> HideAsync(Guid organizationId, Guid locationId, Guid reviewId, CancellationToken cancellationToken) =>
        authorizedClient.SendJsonAsync(
            HttpMethod.Post, $"{BasePath(organizationId, locationId)}/{reviewId}/hide", body: null, cancellationToken);

    public Task<ApiResult> UnhideAsync(Guid organizationId, Guid locationId, Guid reviewId, CancellationToken cancellationToken) =>
        authorizedClient.SendJsonAsync(
            HttpMethod.Post, $"{BasePath(organizationId, locationId)}/{reviewId}/unhide", body: null, cancellationToken);
}