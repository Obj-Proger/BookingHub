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

    /// <summary>The employee this member corresponds to. Required for <see cref="OrganizationRole.Employee"/>, null for every other role.</summary>
    public Guid? EmployeeId { get; private set; }

    private OrganizationMember(Guid id, Guid organizationId, Guid userId, OrganizationRole role, Guid? locationId, Guid? employeeId)
        : base(id)
    {
        OrganizationId = organizationId;
        UserId = userId;
        Role = role;
        LocationId = locationId;
        EmployeeId = employeeId;
    }

    private OrganizationMember()
    {
    }

    public static Result<OrganizationMember> Create(
        Guid organizationId, Guid userId, OrganizationRole role, Guid? locationId = null, Guid? employeeId = null)
    {
        var organizationIdResult = Guard.NotEmpty(organizationId, "OrganizationMember.OrganizationIdEmpty", "OrganizationId");
        if (organizationIdResult.IsFailure)
            return Result.Failure<OrganizationMember>(organizationIdResult.Error);

        var userIdResult = Guard.NotEmpty(userId, "OrganizationMember.UserIdEmpty", "UserId");
        if (userIdResult.IsFailure)
            return Result.Failure<OrganizationMember>(userIdResult.Error);

        var scopeResult = ValidateScope(role, locationId, employeeId);
        if (scopeResult.IsFailure)
            return Result.Failure<OrganizationMember>(scopeResult.Error);

        return new OrganizationMember(Guid.CreateVersion7(), organizationId, userId, role, locationId, employeeId);
    }

    public Result ChangeRole(OrganizationRole newRole, Guid? locationId = null, Guid? employeeId = null)
    {
        var scopeResult = ValidateScope(newRole, locationId, employeeId);
        if (scopeResult.IsFailure)
            return scopeResult;

        Role = newRole;
        LocationId = locationId;
        EmployeeId = employeeId;
        return Result.Success();
    }

    private static Result ValidateScope(OrganizationRole role, Guid? locationId, Guid? employeeId)
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

        if (role == OrganizationRole.Employee)
        {
            if (employeeId is null || employeeId == Guid.Empty)
                return Result.Failure(DomainErrors.OrganizationMember.EmployeeRequiredForEmployeeRole);
        }
        else if (employeeId is not null)
        {
            return Result.Failure(DomainErrors.OrganizationMember.EmployeeNotAllowedForRole);
        }

        return Result.Success();
    }
}