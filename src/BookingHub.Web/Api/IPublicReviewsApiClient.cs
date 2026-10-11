using BookingHub.Web.Api.Contracts;

namespace BookingHub.Web.Api;

public interface IPublicReviewsApiClient
{
    Task<ReviewSubmittedResponse?> SubmitReviewAsync(Guid bookingId, string? token, int rating, string? comment, CancellationToken cancellationToken);
}