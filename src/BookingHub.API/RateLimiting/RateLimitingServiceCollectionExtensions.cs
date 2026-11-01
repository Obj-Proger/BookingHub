using System.Globalization;
using System.Security.Claims;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.JsonWebTokens;

namespace BookingHub.API.RateLimiting;

internal static class RateLimitingServiceCollectionExtensions
{
    private static readonly TimeSpan Window = TimeSpan.FromMinutes(1);

    public static IServiceCollection AddApiRateLimiting(this IServiceCollection services, IConfiguration configuration)
    {
        var limits = configuration.GetSection(RateLimitingOptions.SectionName).Get<RateLimitingOptions>() ?? new RateLimitingOptions();

        return services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.OnRejected = WriteRejectionAsync;

            options.AddPolicy(RateLimitPolicies.PublicRead, context => PerClientAddress(context, limits.PublicReadPermitLimit));
            options.AddPolicy(RateLimitPolicies.PublicWrite, context => PerClientAddress(context, limits.PublicWritePermitLimit));
            options.AddPolicy(RateLimitPolicies.SignIn, context => PerClientAddress(context, limits.SignInPermitLimit));
            options.AddPolicy(RateLimitPolicies.Session, context => PerClientAddress(context, limits.SessionPermitLimit));

            // Staff traffic all arrives from the Web app's address, so it is counted per user instead.
            // Anonymous requests are left to the endpoint policies above.
            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
                context.User.FindFirstValue(JwtRegisteredClaimNames.Sub) is { } userId
                    ? RateLimitPartition.GetSlidingWindowLimiter(userId, _ => CreateWindow(limits.AuthenticatedPermitLimit))
                    : RateLimitPartition.GetNoLimiter(string.Empty));
        });
    }

    private static RateLimitPartition<string> PerClientAddress(HttpContext context, int permitLimit) =>
        RateLimitPartition.GetSlidingWindowLimiter(
            context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => CreateWindow(permitLimit));

    private static SlidingWindowRateLimiterOptions CreateWindow(int permitLimit) => new()
    {
        Window = Window,
        SegmentsPerWindow = 4,
        PermitLimit = permitLimit,
        QueueLimit = 0
    };

    /// <summary>A problem response like every other error, so clients can show its detail; plus Retry-After when known.</summary>
    private static async ValueTask WriteRejectionAsync(OnRejectedContext context, CancellationToken cancellationToken)
    {
        var httpContext = context.HttpContext;

        if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
        {
            httpContext.Response.Headers.RetryAfter =
                ((int)Math.Ceiling(retryAfter.TotalSeconds)).ToString(CultureInfo.InvariantCulture);
        }

        await httpContext.RequestServices.GetRequiredService<IProblemDetailsService>().WriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails =
            {
                Status = StatusCodes.Status429TooManyRequests,
                Title = "Too many requests",
                Detail = "Too many requests. Please wait a moment and try again."
            }
        });
    }
}