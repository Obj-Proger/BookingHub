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
        public static readonly Error SlugAlreadyTaken = Error.Conflict("Organization.SlugAlreadyTaken", "This slug is already in use by another organization.");
        public static readonly Error NotFound = Error.NotFound("Organization.NotFound", "Organization not found.");
        public static readonly Error OnlyOwnerCanDelete = Error.Forbidden("Organization.OnlyOwnerCanDelete", "Only the organization's Owner can delete it.");
    }

    public static class Authorization
    {
        public static readonly Error NotAMember = Error.Forbidden("Authorization.NotAMember", "You are not a member of this organization.");
        public static readonly Error InsufficientRole = Error.Forbidden("Authorization.InsufficientRole", "Your role does not grant access to this operation.");
        public static readonly Error FinancialAccessDisabled = Error.Forbidden("Authorization.FinancialAccessDisabled", "The organization's Owner has not enabled financial data access for Administrators.");
    }

    public static class Location
    {
        public static readonly Error NotFound = Error.NotFound("Location.NotFound", "Location not found.");
    }

    public static class Employee
    {
        public static readonly Error NotFound = Error.NotFound("Employee.NotFound", "Employee not found.");
        public static readonly Error NotAssignedToLocation = Error.Conflict("Employee.NotAssignedToLocation", "This employee does not work at this location.");
        public static readonly Error CallerIsNotAnEmployee = Error.Forbidden("Employee.CallerIsNotAnEmployee", "The caller has no Employee membership in this organization.");
    }

    public static class EmployeeLocationAssignment
    {
        public static readonly Error NotFound = Error.NotFound("EmployeeLocationAssignment.NotFound", "Employee assignment not found.");
        public static readonly Error AlreadyAssigned = Error.Conflict("EmployeeLocationAssignment.AlreadyAssigned", "This employee is already assigned to this location.");
    }

    public static class Service
    {
        public static readonly Error NotFound = Error.NotFound("Service.NotFound", "Service not found.");
    }

    public static class LocationServiceOverride
    {
        public static readonly Error NotFound = Error.NotFound("LocationServiceOverride.NotFound", "Location service price override not found.");
        public static readonly Error AlreadyExists = Error.Conflict("LocationServiceOverride.AlreadyExists", "A price override for this service at this location already exists.");
        public static readonly Error CurrencyMismatch = Error.Validation("LocationServiceOverride.CurrencyMismatch", "The override currency must match the service's base price currency.");
    }

    public static class Booking
    {
        public static readonly Error SlotNotAvailable = Error.Conflict("Booking.SlotNotAvailable", "The requested time slot is no longer available.");
        public static readonly Error NoOccurrenceAvailable = Error.Conflict("Booking.NoOccurrenceAvailable", "None of the requested recurring occurrences are available.");
        public static readonly Error NotFound = Error.NotFound("Booking.NotFound", "Booking not found.");
        public static readonly Error InvalidConfirmationToken = Error.Forbidden("Booking.InvalidConfirmationToken", "The confirmation token is invalid.");
        public static readonly Error InvalidManagementToken = Error.Forbidden("Booking.InvalidManagementToken", "The management token is invalid.");
        public static readonly Error CancellationDeadlinePassed = Error.Conflict("Booking.CancellationDeadlinePassed", "This booking can no longer be cancelled or rescheduled — the deadline has passed.");
    }

    public static class WaitlistEntry
    {
        public static readonly Error EmployeeNotFound = Error.NotFound("WaitlistEntry.EmployeeNotFound", "The requested employee was not found.");
        public static readonly Error NotFound = Error.NotFound("WaitlistEntry.NotFound", "Waitlist entry not found.");
        public static readonly Error InvalidManagementToken = Error.Forbidden("WaitlistEntry.InvalidManagementToken", "The management token is invalid.");
    }

    public static class Review
    {
        public static readonly Error NotFound = Error.NotFound("Review.NotFound", "Review not found.");
        public static readonly Error BookingNotCompleted = Error.Conflict("Review.BookingNotCompleted", "Only a completed booking can be reviewed.");
        public static readonly Error AlreadyExists = Error.Conflict("Review.AlreadyExists", "A review has already been submitted for this booking.");
    }

    public static class OrganizationMember
    {
        public static readonly Error NotFound = Error.NotFound("OrganizationMember.NotFound", "Organization member not found.");
        public static readonly Error AlreadyMember = Error.Conflict("OrganizationMember.AlreadyMember", "This user is already a member of the organization.");
        public static readonly Error OnlyOwnerCanManageOwnerRole = Error.Forbidden("OrganizationMember.OnlyOwnerCanManageOwnerRole", "Only an existing Owner can grant, change, or revoke the Owner role.");
        public static readonly Error CannotRemoveLastOwner = Error.Conflict("OrganizationMember.CannotRemoveLastOwner", "The organization must always have at least one Owner.");
        public static readonly Error UserNotFound = Error.NotFound("OrganizationMember.UserNotFound", "This person has no BookingHub account yet. They need to create one first.");
    }

    public static class RecurringSchedule
    {
        public static readonly Error NotFound = Error.NotFound("RecurringSchedule.NotFound", "Recurring schedule entry not found.");
        public static readonly Error Overlaps = Error.Conflict("RecurringSchedule.Overlaps", "This time range overlaps with an existing recurring schedule entry on the same day.");
    }

    public static class ScheduleException
    {
        public static readonly Error NotFound = Error.NotFound("ScheduleException.NotFound", "Schedule exception not found.");
        public static readonly Error AlreadyExists = Error.Conflict("ScheduleException.AlreadyExists", "A schedule exception already exists for this date.");
    }
    public static class Client
    {
        public static readonly Error NotFound = Error.NotFound("Client.NotFound", "Client not found.");
    }
}