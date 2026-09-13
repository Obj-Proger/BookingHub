using BookingHub.Application.Common.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace BookingHub.API.Common;

/// <summary>
/// Base for every controller under api/v1/public/{organizationSlug} — resolves the slug (already
/// looked up once by TenantResolutionMiddleware into ICurrentTenant, not looked up again here)
/// into <see cref="OrganizationId"/> before any action runs, or short-circuits with 404 if the
/// slug didn't match a real organization. Implementing IAsyncActionFilter directly on a
/// controller is a supported ASP.NET Core MVC convention — the framework picks it up automatically
/// for every action on this class and its subclasses, no attribute or registration needed.
/// </summary>
[AllowAnonymous]
public abstract class PublicApiControllerBase(ICurrentTenant currentTenant) : ApiControllerBase, IAsyncActionFilter
{
    protected Guid OrganizationId { get; private set; }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (currentTenant.OrganizationId is null)
        {
            context.Result = NotFound();
            return;
        }

        OrganizationId = currentTenant.OrganizationId.Value;
        await next();
    }
}