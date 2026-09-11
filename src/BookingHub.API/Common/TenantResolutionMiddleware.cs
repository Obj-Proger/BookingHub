using BookingHub.Application.Common.Security;
using BookingHub.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BookingHub.API.Common;

/// <summary>
/// Resolves which organization the current request belongs to (from {organizationId} or
/// {organizationSlug}), stamps it onto the PostgreSQL session for RLS via set_config inside a
/// transaction spanning the whole request, and exposes it through TenantContext/ICurrentTenant.
/// A no-op — no transaction opened at all — for requests with neither route value (/auth,
/// /health), since those never touch organization-scoped tables.
/// </summary>
internal sealed class TenantResolutionMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, TenantContext tenantContext, ApplicationDbContext dbContext)
    {
        var organizationId = await ResolveOrganizationIdAsync(context, dbContext);

        if (organizationId is null)
        {
            await next(context);
            return;
        }

        tenantContext.OrganizationId = organizationId;

        await using var transaction = await dbContext.Database.BeginTransactionAsync(context.RequestAborted);

        // set_config(..., true) is the function form of SET LOCAL — the value is always bound
        // as a query parameter here (FormattableString interpolation), never string-concatenated.
        await dbContext.Database.ExecuteSqlInterpolatedAsync(
            $"SELECT set_config('app.current_organization_id', {organizationId.Value.ToString()}, true)",
            context.RequestAborted);

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
}