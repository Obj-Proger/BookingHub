using System.Net.Http.Json;
using BookingHub.Web.Api.Contracts;

namespace BookingHub.Web.Api;

internal sealed class PublicWaitlistApiClient(HttpClient httpClient) : IPublicWaitlistApiClient
{
    public async Task<WaitlistEntryCreatedResponse?> JoinAsync(string organizationSlug, JoinWaitlistRequest request, CancellationToken cancellationToken)
    {
        var response = await httpClient.PostAsJsonAsync($"api/v1/public/{organizationSlug}/waitlist", request, cancellationToken);
        return response.IsSuccessStatusCode
            ? await response.Content.ReadFromJsonAsync<WaitlistEntryCreatedResponse>(cancellationToken: cancellationToken)
            : null;
    }

    public async Task<bool> ConfirmOfferAsync(Guid waitlistEntryId, string? token, CancellationToken cancellationToken)
    {
        var response = await httpClient.PostAsJsonAsync(
            $"api/v1/public/waitlist/{waitlistEntryId}/confirm", new ConfirmWaitlistOfferRequest(token), cancellationToken);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> LeaveAsync(Guid waitlistEntryId, string? token, CancellationToken cancellationToken)
    {
        var response = await httpClient.PostAsJsonAsync(
            $"api/v1/public/waitlist/{waitlistEntryId}/leave", new LeaveWaitlistRequest(token), cancellationToken);
        return response.IsSuccessStatusCode;
    }
}