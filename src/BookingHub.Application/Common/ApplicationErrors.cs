namespace BookingHub.Application.Common;

/// <summary>
/// Errors that require infrastructure knowledge (existence in the database, uniqueness)
/// and therefore cannot be expressed inside Domain — see <c>DomainErrors</c> for
/// errors that a Domain factory/method can determine on its own.
/// </summary>
public static class ApplicationErrors
{
    public static class Organization
    {
        public static readonly Error SlugAlreadyTaken = new("Organization.SlugAlreadyTaken", "This slug is already in use by another organization.", ErrorType.Conflict);
        public static readonly Error NotFound = new("Organization.NotFound", "Organization not found.", ErrorType.NotFound);
    }

    public static class Authorization
    {
        public static readonly Error NotAMember = new("Authorization.NotAMember", "You are not a member of this organization.", ErrorType.Forbidden);
        public static readonly Error InsufficientRole = new("Authorization.InsufficientRole", "Your role does not grant access to this operation.", ErrorType.Forbidden);
    }

    public static class Location
    {
        public static readonly Error NotFound = new("Location.NotFound", "Location not found.", ErrorType.NotFound);
    }

    public static class Employee
    {
        public static readonly Error NotFound = new("Employee.NotFound", "Employee not found.", ErrorType.NotFound);
        public static readonly Error NotAssignedToLocation = new("Employee.NotAssignedToLocation", "This employee does not work at this location.", ErrorType.Conflict);
    }

    public static class EmployeeLocationAssignment
    {
        public static readonly Error NotFound = new("EmployeeLocationAssignment.NotFound", "Employee assignment not found.", ErrorType.NotFound);
    }

    public static class Service
    {
        public static readonly Error NotFound = new("Service.NotFound", "Service not found.", ErrorType.NotFound);
    }

    public static class LocationServiceOverride
    {
        public static readonly Error NotFound = new("LocationServiceOverride.NotFound", "Location service price override not found.", ErrorType.NotFound);
        public static readonly Error AlreadyExists = new("LocationServiceOverride.AlreadyExists", "A price override for this service at this location already exists.", ErrorType.Conflict);
        public static readonly Error CurrencyMismatch = new("LocationServiceOverride.CurrencyMismatch", "The override currency must match the service's base price currency.", ErrorType.Validation);
    }

    public static class Booking
    {
        public static readonly Error SlotNotAvailable = new("Booking.SlotNotAvailable", "The requested time slot is no longer available.", ErrorType.Conflict);
        public static readonly Error NotFound = new("Booking.NotFound", "Booking not found.", ErrorType.NotFound);
        public static readonly Error InvalidConfirmationToken = new("Booking.InvalidConfirmationToken", "The confirmation token is invalid.", ErrorType.Forbidden);
        public static readonly Error InvalidManagementToken = new("Booking.InvalidManagementToken", "The management token is invalid.", ErrorType.Forbidden);
        public static readonly Error CancellationDeadlinePassed = new("Booking.CancellationDeadlinePassed", "This booking can no longer be cancelled or rescheduled — the deadline has passed.", ErrorType.Conflict);
    }

    public static class WaitlistEntry
    {
        public static readonly Error EmployeeNotFound = new("WaitlistEntry.EmployeeNotFound", "The requested employee was not found.", ErrorType.NotFound);
        public static readonly Error NotFound = new("WaitlistEntry.NotFound", "Waitlist entry not found.", ErrorType.NotFound);
        public static readonly Error InvalidManagementToken = new("WaitlistEntry.InvalidManagementToken", "The management token is invalid.", ErrorType.Forbidden);
    }

    public static class Review
    {
        public static readonly Error NotFound = new("Review.NotFound", "Review not found.", ErrorType.NotFound);
        public static readonly Error BookingNotCompleted = new("Review.BookingNotCompleted", "Only a completed booking can be reviewed.", ErrorType.Conflict);
        public static readonly Error AlreadyExists = new("Review.AlreadyExists", "A review has already been submitted for this booking.", ErrorType.Conflict);
    }

    public static class OrganizationMember
    {
        public static readonly Error NotFound = new("OrganizationMember.NotFound", "Organization member not found.", ErrorType.NotFound);
        public static readonly Error AlreadyMember = new("OrganizationMember.AlreadyMember", "This user is already a member of the organization.", ErrorType.Conflict);
        public static readonly Error OnlyOwnerCanManageOwnerRole = new("OrganizationMember.OnlyOwnerCanManageOwnerRole", "Only an existing Owner can grant, change, or revoke the Owner role.", ErrorType.Forbidden);
        public static readonly Error CannotRemoveLastOwner = new("OrganizationMember.CannotRemoveLastOwner", "The organization must always have at least one Owner.", ErrorType.Conflict);
    }

    public static class RecurringSchedule
    {
        public static readonly Error NotFound = new("RecurringSchedule.NotFound", "Recurring schedule entry not found.", ErrorType.NotFound);
        public static readonly Error Overlaps = new("RecurringSchedule.Overlaps", "This time range overlaps with an existing recurring schedule entry on the same day.", ErrorType.Conflict);
    }

    public static class ScheduleException
    {
        public static readonly Error NotFound = new("ScheduleException.NotFound", "Schedule exception not found.", ErrorType.NotFound);
        public static readonly Error AlreadyExists = new("ScheduleException.AlreadyExists", "A schedule exception already exists for this date.", ErrorType.Conflict);
    }
}