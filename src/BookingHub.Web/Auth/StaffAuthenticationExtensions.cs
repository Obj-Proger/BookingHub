using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Authorization;

namespace BookingHub.Web.Auth;

internal static class StaffAuthenticationExtensions
{
    public static IServiceCollection AddStaffAuthentication(this IServiceCollection services)
    {
        services.AddSingleton<ITokenStore, InMemoryTokenStore>();
        services.AddScoped<StaffSessionManager>();
        services.AddScoped<AuthenticationStateProvider, SessionAuthenticationStateProvider>();
        services.AddCascadingAuthenticationState();

        services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(options =>
            {
                options.Cookie.Name = "bookinghub.staff";
                options.Cookie.HttpOnly = true;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                options.Cookie.SameSite = SameSiteMode.Lax;
                options.LoginPath = "/login";
                options.ReturnUrlParameter = "returnUrl";
                options.ExpireTimeSpan = InMemoryTokenStore.IdleTimeout;
                options.SlidingExpiration = true;
                options.Events.OnValidatePrincipal = ValidateSessionAsync;
            });

        services.AddAuthorization();

        return services;
    }

    public static IEndpointRouteBuilder MapStaffLogout(this IEndpointRouteBuilder endpoints)
    {
        // Binding the form makes the endpoint validate the antiforgery token of the posting form.
        endpoints.MapPost("/logout", async (
            HttpContext httpContext,
            StaffSessionManager sessionManager,
            IFormCollection form,
            CancellationToken cancellationToken) =>
        {
            await sessionManager.SignOutAsync(httpContext, cancellationToken);
            return Results.LocalRedirect("/login");
        });

        return endpoints;
    }

    /// <summary>Rejects a cookie whose server-side session no longer exists (e.g. after an app restart).</summary>
    private static async Task ValidateSessionAsync(CookieValidatePrincipalContext context)
    {
        var sessionId = context.Principal?.GetSessionId();
        var tokenStore = context.HttpContext.RequestServices.GetRequiredService<ITokenStore>();

        if (sessionId is not null && tokenStore.TryGet(sessionId, out _))
            return;

        context.RejectPrincipal();
        await context.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    }
}