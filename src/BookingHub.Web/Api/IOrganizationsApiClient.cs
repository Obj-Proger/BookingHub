using BookingHub.Web.Api.Contracts;

namespace BookingHub.Web.Api;

public interface IOrganizationsApiClient
{
    Task<ApiResult<OrganizationCreatedResponse>> CreateAsync(string? name, string? slug, CancellationToken cancellationToken);
}