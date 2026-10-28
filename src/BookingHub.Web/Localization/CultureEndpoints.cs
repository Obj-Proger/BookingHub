using BookingHub.Web.Auth;
using Microsoft.AspNetCore.Localization;

namespace BookingHub.Web.Localization;

internal static class CultureEndpoints
{
    /// <summary>Remembers the chosen language in a cookie and returns to the page the visitor came from.</summary>
    public static IEndpointRouteBuilder MapCultureSwitch(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/culture/set", (HttpContext httpContext, string? culture, string? returnUrl) =>
        {
            if (SupportedCultures.IsSupported(culture))
            {
                httpContext.Response.Cookies.Append(
                    CookieRequestCultureProvider.DefaultCookieName,
                    CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture!)),
                    new CookieOptions
                    {
                        Expires = DateTimeOffset.UtcNow.AddYears(1),
                        IsEssential = true,
                        SameSite = SameSiteMode.Lax,
                        Secure = httpContext.Request.IsHttps
                    });
            }

            return Results.LocalRedirect(LocalUrl.OrDefault(returnUrl, "/"));
        });

        return endpoints;
    }
}