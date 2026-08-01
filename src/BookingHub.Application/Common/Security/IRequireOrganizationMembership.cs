namespace BookingHub.Application.Common.Security;

/// <summary>Marks a request that requires the caller to belong to the organization, in any role.</summary>
public interface IRequireOrganizationMembership
{
    Guid OrganizationId { get; }
}