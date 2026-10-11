using System.Net.Http.Json;
using BookingHub.Web.Api.Contracts;

namespace BookingHub.Web.Api;

internal sealed class PublicReviewsApiClient(HttpClient httpClient) : IPublicReviewsApiClient
{
    public async Task<ReviewSubmittedResponse?> SubmitReviewAsync(Guid bookingId, string? token, int rating, string? comment, CancellationToken cancellationToken)
    {
        var response = await httpClient.PostAsJsonAsync(
            $"api/v1/public/bookings/{bookingId}/review", new SubmitReviewRequest(token, rating, comment), cancellationToken);
        return response.IsSuccessStatusCode
            ? await response.Content.ReadFromJsonAsync<ReviewSubmittedResponse>(cancellationToken: cancellationToken)
            : null;
    }
}