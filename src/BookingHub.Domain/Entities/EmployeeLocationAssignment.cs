namespace BookingHub.Domain.Entities;

/// <summary>
/// Assigns an <see cref="Employee"/> to a <see cref="Location"/>, anchoring that
/// employee's <c>RecurringSchedule</c> and <c>ScheduleException</c> entries.
/// </summary>
public sealed class EmployeeLocationAssignment : BaseEntity
{
    public Guid EmployeeId { get; private set; }
    public Guid LocationId { get; private set; }
    public bool IsActive { get; private set; }

    private EmployeeLocationAssignment(Guid id, Guid employeeId, Guid locationId) : base(id)
    {
        EmployeeId = employeeId;
        LocationId = locationId;
        IsActive = true;
    }

    private EmployeeLocationAssignment()
    {
    }

    public static Result<EmployeeLocationAssignment> Create(Guid employeeId, Guid locationId)
    {
        var employeeIdResult = Guard.NotEmpty(employeeId, "EmployeeLocationAssignment.EmployeeIdEmpty", "EmployeeId");
        if (employeeIdResult.IsFailure)
            return Result.Failure<EmployeeLocationAssignment>(employeeIdResult.Error);

        var locationIdResult = Guard.NotEmpty(locationId, "EmployeeLocationAssignment.LocationIdEmpty", "LocationId");
        if (locationIdResult.IsFailure)
            return Result.Failure<EmployeeLocationAssignment>(locationIdResult.Error);

        return new EmployeeLocationAssignment(Guid.CreateVersion7(), employeeId, locationId);
    }

    /// <summary>Unassigns the employee from the location without deleting their schedule history.</summary>
    public void Deactivate() => IsActive = false;

    public void Reactivate() => IsActive = true;
}