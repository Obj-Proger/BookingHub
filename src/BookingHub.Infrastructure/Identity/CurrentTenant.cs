using BookingHub.Application.Common.Security;
using Microsoft.AspNetCore.Http;

namespace BookingHub.Infrastructure.Identity;

internal sealed class CurrentTenant(IHttpContextAccessor httpContextAccessor) : ICurrentTenant
{
    public Guid? OrganizationId
    {
        get
        {
            var routeValue = httpContextAccessor.HttpContext?.Request.RouteValues["organizationId"]?.ToString();
            return Guid.TryParse(routeValue, out var organizationId) ? organizationId : null;
        }
    }
}