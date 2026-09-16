using BookingHub.Application.Common.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace BookingHub.API.Common;

[AllowAnonymous]
public abstract class PublicApiControllerBase(ICurrentTenant currentTenant) : ApiControllerBase, IAsyncActionFilter
{
    protected Guid OrganizationId { get; private set; }

    [NonAction]
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