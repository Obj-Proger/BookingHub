using BookingHub.Web.Api.Contracts;

namespace BookingHub.Web.Api;

public interface IStaffReviewsApiClient
{
    /// <summary>Reviews of a location, hidden ones included, newest first.</summary>
    Task<ApiResult<IReadOnlyList<ReviewModerationResponse>>> GetLocationReviewsAsync(
        Guid organizationId, Guid locationId, int skip, int take, CancellationToken cancellationToken);

    Task<ApiResult> HideAsync(Guid organizationId, Guid locationId, Guid reviewId, CancellationToken cancellationToken);

    Task<ApiResult> UnhideAsync(Guid organizationId, Guid locationId, Guid reviewId, CancellationToken cancellationToken);
}