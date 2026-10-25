using BookingHub.Web.Api.Contracts;
using BookingHub.Web.Auth;

namespace BookingHub.Web.Api;

internal sealed class OrganizationsApiClient(AuthorizedApiClient authorizedClient) : IOrganizationsApiClient
{
    private const string BasePath = "api/v1/organizations";

    public Task<ApiResult<OrganizationCreatedResponse>> CreateAsync(string? name, string? slug, CancellationToken cancellationToken) =>
        authorizedClient.SendJsonAsync<OrganizationCreatedResponse>(
            HttpMethod.Post, BasePath, new CreateOrganizationRequest(name, slug), cancellationToken);

    public Task<ApiResult<OrganizationResponse>> GetAsync(Guid organizationId, CancellationToken cancellationToken) =>
        authorizedClient.GetJsonAsync<OrganizationResponse>($"{BasePath}/{organizationId}", cancellationToken);

    public Task<ApiResult> RenameAsync(Guid organizationId, string? newName, CancellationToken cancellationToken) =>
        authorizedClient.SendJsonAsync(
            HttpMethod.Put, $"{BasePath}/{organizationId}/name", new RenameOrganizationRequest(newName), cancellationToken);

    public Task<ApiResult> UpdateCancellationDeadlineAsync(Guid organizationId, TimeSpan deadline, CancellationToken cancellationToken) =>
        authorizedClient.SendJsonAsync(
            HttpMethod.Put, $"{BasePath}/{organizationId}/settings/cancellation-deadline",
            new UpdateCancellationDeadlineRequest(deadline), cancellationToken);

    public Task<ApiResult> UpdateSchedulingWindowAsync(
        Guid organizationId, SchedulingWindow window, TimeSpan value, CancellationToken cancellationToken) =>
        authorizedClient.SendJsonAsync(
            HttpMethod.Put, $"{BasePath}/{organizationId}/settings/{PathSegmentOf(window)}",
            new UpdateSchedulingWindowRequest(value), cancellationToken);

    public Task<ApiResult> SetAdministratorFinancialAccessAsync(Guid organizationId, bool enabled, CancellationToken cancellationToken) =>
        authorizedClient.SendJsonAsync(
            HttpMethod.Put, $"{BasePath}/{organizationId}/settings/administrator-financial-access",
            new SetAdministratorFinancialAccessRequest(enabled), cancellationToken);

    public Task<ApiResult> DeleteAsync(Guid organizationId, CancellationToken cancellationToken) =>
        authorizedClient.SendJsonAsync(HttpMethod.Delete, $"{BasePath}/{organizationId}", body: null, cancellationToken);

    private static string PathSegmentOf(SchedulingWindow window) => window switch
    {
        SchedulingWindow.PendingConfirmation => "pending-confirmation-window",
        SchedulingWindow.AutoComplete => "auto-complete-window",
        SchedulingWindow.WaitlistOffer => "waitlist-offer-window",
        _ => throw new ArgumentOutOfRangeException(nameof(window), window, null)
    };
}