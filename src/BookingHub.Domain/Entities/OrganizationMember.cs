using BookingHub.Domain.Enums;

namespace BookingHub.Domain.Entities;

/// <summary>
/// Grants a user a specific <see cref="OrganizationRole"/> within an organization —
/// the single source of truth for authorization checks, independent of whether the
/// same person also has an <see cref="Entities.Employee"/> record for being bookable.
/// </summary>
public sealed class OrganizationMember : BaseEntity
{
    public Guid OrganizationId { get; private set; }
    public Guid UserId { get; private set; }
    public OrganizationRole Role { get; private set; }

    /// <summary>The single location this member is scoped to. Required for <see cref="OrganizationRole.LocationManager"/>, null for every other role.</summary>
    public Guid? LocationId { get; private set; }

    private OrganizationMember(Guid id, Guid organizationId, Guid userId, OrganizationRole role, Guid? locationId)
        : base(id)
    {
        OrganizationId = organizationId;
        UserId = userId;
        Role = role;
        LocationId = locationId;
    }

    private OrganizationMember()
    {
    }

    public static Result<OrganizationMember> Create(Guid organizationId, Guid userId, OrganizationRole role, Guid? locationId = null)
    {
        var organizationIdResult = Guard.NotEmpty(organizationId, "OrganizationMember.OrganizationIdEmpty", "OrganizationId");
        if (organizationIdResult.IsFailure)
            return Result.Failure<OrganizationMember>(organizationIdResult.Error);

        var userIdResult = Guard.NotEmpty(userId, "OrganizationMember.UserIdEmpty", "UserId");
        if (userIdResult.IsFailure)
            return Result.Failure<OrganizationMember>(userIdResult.Error);

        var locationScopeResult = ValidateLocationScope(role, locationId);
        if (locationScopeResult.IsFailure)
            return Result.Failure<OrganizationMember>(locationScopeResult.Error);

        return new OrganizationMember(Guid.CreateVersion7(), organizationId, userId, role, locationId);
    }

    public Result ChangeRole(OrganizationRole newRole, Guid? locationId = null)
    {
        var locationScopeResult = ValidateLocationScope(newRole, locationId);
        if (locationScopeResult.IsFailure)
            return locationScopeResult;

        Role = newRole;
        LocationId = locationId;
        return Result.Success();
    }

    private static Result ValidateLocationScope(OrganizationRole role, Guid? locationId)
    {
        if (role == OrganizationRole.LocationManager)
        {
            if (locationId is null || locationId == Guid.Empty)
                return Result.Failure(DomainErrors.OrganizationMember.LocationRequiredForLocationManager);
        }
        else if (locationId is not null)
        {
            return Result.Failure(DomainErrors.OrganizationMember.LocationNotAllowedForRole);
        }

        return Result.Success();
    }
}