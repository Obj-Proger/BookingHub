namespace BookingHub.Application.Common.Security;

/// <summary>
/// Narrows the current unit of work to one organization: from this point on, data access sees and
/// writes only that organization's rows. Entry points (an API request, a background job) enter the
/// scope themselves; a handler enters it only when the organization does not exist before it runs
/// (creating an organization).
/// </summary>
public interface ITenantScope
{
    Task EnterAsync(Guid organizationId, CancellationToken cancellationToken);
}