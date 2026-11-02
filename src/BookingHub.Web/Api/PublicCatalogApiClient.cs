using System.Net;
using BookingHub.Web.Api.Contracts;

namespace BookingHub.Web.Api;

internal sealed class PublicCatalogApiClient(HttpClient httpClient) : IPublicCatalogApiClient
{
    public Task<ApiResult<IReadOnlyList<PublicLocationResponse>>> GetLocationsAsync(string organizationSlug, CancellationToken cancellationToken) =>
        httpClient.GetApiResultAsync<IReadOnlyList<PublicLocationResponse>>(
            $"api/v1/public/{organizationSlug}/locations", cancellationToken);

    /// <summary>Resolved from the public location list — the API has no single-location endpoint.</summary>
    public async Task<ApiResult<PublicLocationResponse>> GetLocationAsync(string organizationSlug, Guid locationId, CancellationToken cancellationToken)
    {
        var locations = await GetLocationsAsync(organizationSlug, cancellationToken);
        if (!locations.IsSuccess)
            return ApiResult<PublicLocationResponse>.Failure(locations.Error!);

        var location = locations.Value.FirstOrDefault(l => l.LocationId == locationId);
        return location is null
            ? ApiResult<PublicLocationResponse>.Failure(new ApiError(HttpStatusCode.NotFound))
            : ApiResult<PublicLocationResponse>.Success(location);
    }

    public Task<ApiResult<IReadOnlyList<PublicServiceResponse>>> GetServicesAsync(string organizationSlug, Guid locationId, CancellationToken cancellationToken) =>
        httpClient.GetApiResultAsync<IReadOnlyList<PublicServiceResponse>>(
            $"api/v1/public/{organizationSlug}/locations/{locationId}/services", cancellationToken);

    public Task<ApiResult<IReadOnlyList<PublicEmployeeResponse>>> GetEmployeesAsync(string organizationSlug, Guid locationId, CancellationToken cancellationToken) =>
        httpClient.GetApiResultAsync<IReadOnlyList<PublicEmployeeResponse>>(
            $"api/v1/public/{organizationSlug}/locations/{locationId}/employees", cancellationToken);
}