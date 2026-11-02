using BookingHub.Web.Api.Contracts;

namespace BookingHub.Web.Api;

public interface IPublicReviewsApiClient
{
    Task<ApiResult<ReviewSubmittedResponse>> SubmitReviewAsync(
        string organizationSlug, Guid bookingId, string? token, int rating, string? comment, CancellationToken cancellationToken);
}