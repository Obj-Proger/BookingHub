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
    }
}