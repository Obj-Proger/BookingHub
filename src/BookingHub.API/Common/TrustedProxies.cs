using System.Net;
using Microsoft.AspNetCore.HttpOverrides;

namespace BookingHub.API.Common;

/// <summary>
/// Lets UseForwardedHeaders take the client address from X-Forwarded-For — but only when the request
/// comes from a configured proxy: the Web app, and a reverse proxy in front of the API if there is
/// one. Loopback is trusted by default, so Web and API work side by side on a dev machine; any
/// other proxy must be listed under ForwardedHeaders:KnownProxies (addresses) or
/// ForwardedHeaders:KnownNetworks (CIDR ranges, e.g. a Docker network). Trusting the header from
/// anyone would let a client claim any address and walk around the per-IP rate limits.
/// </summary>
internal static class TrustedProxies
{
    private const string SectionName = "ForwardedHeaders";

    public static IServiceCollection AddTrustedProxies(this IServiceCollection services, IConfiguration configuration)
    {
        var section = configuration.GetSection(SectionName);

        // Parsed here, not inside Configure, so a typo stops the app at startup with the bad value in the message.
        var proxies = (section.GetSection("KnownProxies").Get<string[]>() ?? []).Select(ParseAddress).ToList();
        var networks = (section.GetSection("KnownNetworks").Get<string[]>() ?? []).Select(ParseNetwork).ToList();

        return services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;

            foreach (var proxy in proxies)
                options.KnownProxies.Add(proxy);

            foreach (var network in networks)
                options.KnownIPNetworks.Add(network);
        });
    }

    private static IPAddress ParseAddress(string value) =>
        IPAddress.TryParse(value, out var address) ? address : throw Invalid(value);

    private static System.Net.IPNetwork ParseNetwork(string value) =>
        System.Net.IPNetwork.TryParse(value, out var network) ? network : throw Invalid(value);

    private static InvalidOperationException Invalid(string value) =>
        new($"{SectionName}: \"{value}\" is not a valid IP address or CIDR range.");
}