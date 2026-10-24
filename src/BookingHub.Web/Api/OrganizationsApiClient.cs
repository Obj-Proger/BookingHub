using System.Net.Http.Json;
using BookingHub.Web.Api.Contracts;
using BookingHub.Web.Auth;

namespace BookingHub.Web.Api;

internal sealed class OrganizationsApiClient(AuthorizedApiClient authorizedClient) : IOrganizationsApiClient
{
    private const string BasePath = "api/v1/organizations";

    public async Task<ApiResult<OrganizationCreatedResponse>> CreateAsync(string? name, string? slug, CancellationToken cancellationToken)
    {
        using var response = await authorizedClient.SendAsync(
            () => CreateRequest(HttpMethod.Post, BasePath, new CreateOrganizationRequest(name, slug)), cancellationToken);

        return await ApiResponseReader.ReadAsync<OrganizationCreatedResponse>(response, cancellationToken);
    }

    public async Task<ApiResult<OrganizationResponse>> GetAsync(Guid organizationId, CancellationToken cancellationToken)
    {
        using var response = await authorizedClient.SendAsync(
            () => CreateRequest(HttpMethod.Get, $"{BasePath}/{organizationId}", body: null), cancellationToken);

        return await ApiResponseReader.ReadAsync<OrganizationResponse>(response, cancellationToken);
    }

    public Task<ApiResult> RenameAsync(Guid organizationId, string? newName, CancellationToken cancellationToken) =>
        SendAsync(HttpMethod.Put, $"{BasePath}/{organizationId}/name", new RenameOrganizationRequest(newName), cancellationToken);

    public Task<ApiResult> UpdateCancellationDeadlineAsync(Guid organizationId, TimeSpan deadline, CancellationToken cancellationToken) =>
        SendAsync(HttpMethod.Put, $"{BasePath}/{organizationId}/settings/cancellation-deadline",
            new UpdateCancellationDeadlineRequest(deadline), cancellationToken);

    public Task<ApiResult> UpdateSchedulingWindowAsync(
        Guid organizationId, SchedulingWindow window, TimeSpan value, CancellationToken cancellationToken) =>
        SendAsync(HttpMethod.Put, $"{BasePath}/{organizationId}/settings/{PathSegmentOf(window)}",
            new UpdateSchedulingWindowRequest(value), cancellationToken);

    public Task<ApiResult> SetAdministratorFinancialAccessAsync(Guid organizationId, bool enabled, CancellationToken cancellationToken) =>
        SendAsync(HttpMethod.Put, $"{BasePath}/{organizationId}/settings/administrator-financial-access",
            new SetAdministratorFinancialAccessRequest(enabled), cancellationToken);

    public Task<ApiResult> DeleteAsync(Guid organizationId, CancellationToken cancellationToken) =>
        SendAsync(HttpMethod.Delete, $"{BasePath}/{organizationId}", body: null, cancellationToken);

    private async Task<ApiResult> SendAsync(HttpMethod method, string uri, object? body, CancellationToken cancellationToken)
    {
        using var response = await authorizedClient.SendAsync(() => CreateRequest(method, uri, body), cancellationToken);
        return await ApiResponseReader.ReadAsync(response, cancellationToken);
    }

    private static HttpRequestMessage CreateRequest(HttpMethod method, string uri, object? body) =>
        new(method, uri)
        {
            Content = body is null ? null : JsonContent.Create(body, body.GetType(), options: ApiJsonOptions.Default)
        };

    private static string PathSegmentOf(SchedulingWindow window) => window switch
    {
        SchedulingWindow.PendingConfirmation => "pending-confirmation-window",
        SchedulingWindow.AutoComplete => "auto-complete-window",
        SchedulingWindow.WaitlistOffer => "waitlist-offer-window",
        _ => throw new ArgumentOutOfRangeException(nameof(window), window, null)
    };
}