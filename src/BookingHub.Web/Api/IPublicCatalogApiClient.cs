using BookingHub.Web.Api.Contracts;

namespace BookingHub.Web.Api;

public interface IPublicCatalogApiClient
{
    Task<IReadOnlyList<PublicLocationResponse>?> GetLocationsAsync(string organizationSlug, CancellationToken cancellationToken);
    Task<IReadOnlyList<PublicServiceResponse>?> GetServicesAsync(string organizationSlug, Guid locationId, CancellationToken cancellationToken);
    Task<IReadOnlyList<PublicEmployeeResponse>?> GetEmployeesAsync(string organizationSlug, Guid locationId, CancellationToken cancellationToken);
}