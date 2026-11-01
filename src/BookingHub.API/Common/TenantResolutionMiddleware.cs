using System.Security.Claims;
using BookingHub.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.JsonWebTokens;

namespace BookingHub.API.Common;

/// <summary>
/// Gives the request its database scope: resolves the organization from the {organizationId} or
/// {organizationSlug} route value, opens one transaction for the whole request and sets the values
/// row-level security reads — the organization, and the signed-in user (so a caller can list their own
/// memberships on /me routes, which name no organization). Requests with neither (sign-in, health)
/// run without a transaction, since they touch no organization-scoped table.
/// Runs after authentication, which is what makes the user known.
/// </summary>
internal sealed class TenantResolutionMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, ApplicationDbContext dbContext, RowLevelSecuritySession session)
    {
        var organizationId = await ResolveOrganizationIdAsync(context, dbContext);
        var userId = GetUserId(context.User);

        if (organizationId is null && userId is null)
        {
            await next(context);
            return;
        }

        await using var transaction = await dbContext.Database.BeginTransactionAsync(context.RequestAborted);

        if (userId is not null)
            await session.SetUserAsync(userId.Value, context.RequestAborted);

        if (organizationId is not null)
            await session.EnterAsync(organizationId.Value, context.RequestAborted);

        try
        {
            await next(context);
            await transaction.CommitAsync(context.RequestAborted);
        }
        catch
        {
            await transaction.RollbackAsync(CancellationToken.None);
            throw;
        }
    }

    private static async Task<Guid?> ResolveOrganizationIdAsync(HttpContext context, ApplicationDbContext dbContext)
    {
        if (context.Request.RouteValues.TryGetValue("organizationId", out var idValue) &&
            Guid.TryParse(idValue?.ToString(), out var organizationId))
        {
            return organizationId;
        }

        if (context.Request.RouteValues.TryGetValue("organizationSlug", out var slugValue) && slugValue is string slug)
        {
            // Organizations itself carries no OrganizationId column and no query filter — it is
            // the tenant root, not a child table — so this lookup needs no special exemption.
            return await dbContext.Organizations
                .Where(o => o.Slug == slug)
                .Select(o => (Guid?)o.Id)
                .FirstOrDefaultAsync(context.RequestAborted);
        }

        return null;
    }

    /// <summary>The JWT "sub" claim; JwtBearer keeps it unmapped (MapInboundClaims = false).</summary>
    private static Guid? GetUserId(ClaimsPrincipal user) =>
        Guid.TryParse(user.FindFirstValue(JwtRegisteredClaimNames.Sub), out var userId) ? userId : null;
}