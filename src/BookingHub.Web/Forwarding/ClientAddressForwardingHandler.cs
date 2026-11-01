using Microsoft.AspNetCore.HttpOverrides;

namespace BookingHub.Web.Forwarding;

/// <summary>
/// Tells the API who the request is really for: every API call leaves from this server, so without
/// X-Forwarded-For the API would rate-limit all guests as a single client. The API honours the
/// header only from addresses it trusts (its ForwardedHeaders configuration).
/// </summary>
internal sealed class ClientAddressForwardingHandler(
    CircuitServicesAccessor circuitServices, IHttpContextAccessor httpContextAccessor)
    : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var clientAddress = GetClientAddress();
        if (clientAddress is not null)
            request.Headers.TryAddWithoutValidation(ForwardedHeadersDefaults.XForwardedForHeaderName, clientAddress);

        return base.SendAsync(request, cancellationToken);
    }

    private string? GetClientAddress() =>
        circuitServices.Services is { } services
            ? services.GetService<ClientAddress>()?.Value // inside an interactive circuit
            : httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString(); // prerendering or an endpoint
}