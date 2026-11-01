using BookingHub.Application.Common.Security;
using Microsoft.EntityFrameworkCore;

namespace BookingHub.Infrastructure.Persistence;

/// <summary>
/// Sets the PostgreSQL session values the row-level security policies read:
/// <c>app.current_organization_id</c> and <c>app.current_user_id</c>. Both are transaction-local
/// (<c>set_config(..., true)</c>), so they only work inside a transaction the caller has opened —
/// the request transaction in the API, a per-organization transaction in a background job.
/// </summary>
public sealed class RowLevelSecuritySession(ApplicationDbContext dbContext, TenantContext tenantContext) : ITenantScope
{
    private const string OrganizationSetting = "app.current_organization_id";
    private const string UserSetting = "app.current_user_id";

    public async Task EnterAsync(Guid organizationId, CancellationToken cancellationToken)
    {
        // The query filters read the same organization, so the two isolation layers always agree.
        tenantContext.OrganizationId = organizationId;
        await SetAsync(OrganizationSetting, organizationId, cancellationToken);
    }

    /// <summary>Lets the caller read their own memberships across organizations (see the member_self_read policy).</summary>
    public Task SetUserAsync(Guid userId, CancellationToken cancellationToken) =>
        SetAsync(UserSetting, userId, cancellationToken);

    private async Task SetAsync(string setting, Guid value, CancellationToken cancellationToken)
    {
        if (dbContext.Database.CurrentTransaction is null)
        {
            throw new InvalidOperationException(
                $"{setting} is transaction-local: open a transaction before setting it, or it is lost after the next statement.");
        }

        // Both the setting name and the value are bound as parameters, never concatenated into the SQL.
        await dbContext.Database.ExecuteSqlInterpolatedAsync(
            $"SELECT set_config({setting}, {value.ToString()}, true)", cancellationToken);
    }
}