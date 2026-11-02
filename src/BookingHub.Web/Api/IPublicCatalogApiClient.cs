using BookingHub.Web.Api.Contracts;

namespace BookingHub.Web.Api;

public interface IPublicCatalogApiClient
{
    Task<ApiResult<IReadOnlyList<PublicLocationResponse>>> GetLocationsAsync(string organizationSlug, CancellationToken cancellationToken);

    /// <summary>A 404 failure when the organization has no such bookable location.</summary>
    Task<ApiResult<PublicLocationResponse>> GetLocationAsync(string organizationSlug, Guid locationId, CancellationToken cancellationToken);

    Task<ApiResult<IReadOnlyList<PublicServiceResponse>>> GetServicesAsync(string organizationSlug, Guid locationId, CancellationToken cancellationToken);

    Task<ApiResult<IReadOnlyList<PublicEmployeeResponse>>> GetEmployeesAsync(string organizationSlug, Guid locationId, CancellationToken cancellationToken);
}