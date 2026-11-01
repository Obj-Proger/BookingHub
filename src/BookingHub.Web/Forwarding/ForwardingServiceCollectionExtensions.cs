using System.Net;
using Microsoft.AspNetCore.Components.Server.Circuits;
using Microsoft.AspNetCore.HttpOverrides;

namespace BookingHub.Web.Forwarding;

internal static class ForwardingServiceCollectionExtensions
{
    private const string SectionName = "ForwardedHeaders";

    /// <summary>Adds the browser's address to every request this app sends to the API.</summary>
    public static IServiceCollection AddClientAddressForwarding(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddSingleton<CircuitServicesAccessor>();
        services.AddScoped<CircuitHandler, CircuitServicesAccessorHandler>();
        services.AddScoped<ClientAddress>();
        services.AddTransient<ClientAddressForwardingHandler>();

        return services.ConfigureHttpClientDefaults(http => http.AddHttpMessageHandler<ClientAddressForwardingHandler>());
    }

    /// <summary>
    /// Lets UseForwardedHeaders take the browser's address from a reverse proxy in front of this app,
    /// but only from proxies listed under ForwardedHeaders:KnownProxies (addresses) or
    /// ForwardedHeaders:KnownNetworks (CIDR ranges); loopback is trusted by default.
    /// </summary>
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