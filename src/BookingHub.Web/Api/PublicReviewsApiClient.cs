using BookingHub.Web.Api.Contracts;

namespace BookingHub.Web.Api;

internal sealed class PublicReviewsApiClient(HttpClient httpClient) : IPublicReviewsApiClient
{
    public Task<ApiResult<ReviewSubmittedResponse>> SubmitReviewAsync(
        string organizationSlug, Guid bookingId, string? token, int rating, string? comment, CancellationToken cancellationToken) =>
        httpClient.PostApiResultAsync<ReviewSubmittedResponse>(
            $"api/v1/public/{organizationSlug}/bookings/{bookingId}/review",
            new SubmitReviewRequest(token, rating, comment), cancellationToken);
}