namespace BookingHub.Domain.Common;

/// <summary>
/// Central catalog of domain-specific <see cref="Error"/> instances, grouped by concept.
/// Codes follow <c>Concept.ErrorName</c> and are part of the API contract (returned as <c>errorCode</c>).
/// </summary>
public static class DomainErrors
{
    public static class Email
    {
        public static readonly Error Empty = Error.Validation("Email.Empty", "Email address cannot be empty.");
        public static readonly Error TooLong = Error.Validation("Email.TooLong", $"Email address cannot exceed {ValueObjects.Email.MaxLength} characters.");
        public static readonly Error InvalidFormat = Error.Validation("Email.InvalidFormat", "Email address is not in a valid format.");
    }

    public static class PhoneNumber
    {
        public static readonly Error Empty = Error.Validation("PhoneNumber.Empty", "Phone number cannot be empty.");
        public static readonly Error InvalidFormat = Error.Validation("PhoneNumber.InvalidFormat", "Phone number must be in international E.164 format, e.g. +14155552671.");
    }

    public static class Money
    {
        public static readonly Error NegativeAmount = Error.Validation("Money.NegativeAmount", "Amount cannot be negative.");
        public static readonly Error InvalidCurrency = Error.Validation("Money.InvalidCurrency", "Currency must be a 3-letter ISO 4217 code.");
    }

    public static class Address
    {
        public static readonly Error Empty = Error.Validation("Address.Empty", "Address cannot be empty.");
        public static readonly Error TooLong = Error.Validation("Address.TooLong", $"Address cannot exceed {ValueObjects.Address.MaxLength} characters.");
    }

    public static class ClientContact
    {
        public static readonly Error NameTooLong = Error.Validation("ClientContact.NameTooLong", $"Name cannot exceed {ValueObjects.ClientContact.MaxNameLength} characters.");
    }

    public static class TimeSlot
    {
        public static readonly Error NotUtc = Error.Validation("TimeSlot.NotUtc", "Start and end must be expressed in UTC.");
        public static readonly Error StartNotBeforeEnd = Error.Validation("TimeSlot.StartNotBeforeEnd", "Start must be earlier than end.");
    }

    public static class DailyHours
    {
        public static readonly Error OpenNotBeforeClose = Error.Validation("DailyHours.OpenNotBeforeClose", "Opening time must be earlier than closing time.");
    }

    public static class WeeklyHours
    {
        public static readonly Error MustCoverAllDays = Error.Validation("WeeklyHours.MustCoverAllDays", "Weekly hours must define exactly one entry for each day of the week.");
    }

    public static class Organization
    {
        public static readonly Error NameEmpty = Error.Validation("Organization.NameEmpty", "Organization name cannot be empty.");
        public static readonly Error NameTooLong = Error.Validation("Organization.NameTooLong", $"Organization name cannot exceed {Entities.Organization.MaxNameLength} characters.");
        public static readonly Error SlugEmpty = Error.Validation("Organization.SlugEmpty", "Organization slug cannot be empty.");
        public static readonly Error SlugInvalidFormat = Error.Validation("Organization.SlugInvalidFormat", $"Slug must be at most {Entities.Organization.MaxSlugLength} characters of lowercase letters, digits and hyphens, and cannot start or end with a hyphen.");
        public static readonly Error SlugReserved = Error.Validation("Organization.SlugReserved", "This address is reserved. Please choose another one.");
        public static readonly Error CancellationDeadlineNegative = Error.Validation("Organization.CancellationDeadlineNegative", "Cancellation deadline cannot be negative.");
        public static readonly Error PendingConfirmationWindowNotPositive = Error.Validation("Organization.PendingConfirmationWindowNotPositive", "Pending confirmation window must be greater than zero.");
        public static readonly Error AutoCompleteWindowNotPositive = Error.Validation("Organization.AutoCompleteWindowNotPositive", "Auto-complete window must be greater than zero.");
        public static readonly Error WaitlistOfferWindowNotPositive = Error.Validation("Organization.WaitlistOfferWindowNotPositive", "Waitlist offer window must be greater than zero.");
    }

    public static class OrganizationMember
    {
        public static readonly Error OrganizationIdEmpty = Error.Validation("OrganizationMember.OrganizationIdEmpty", "OrganizationId is required.");
        public static readonly Error UserIdEmpty = Error.Validation("OrganizationMember.UserIdEmpty", "UserId is required.");
        public static readonly Error LocationRequiredForLocationManager = Error.Validation("OrganizationMember.LocationRequiredForLocationManager", "A location must be specified when the role is LocationManager.");
        public static readonly Error LocationNotAllowedForRole = Error.Validation("OrganizationMember.LocationNotAllowedForRole", "A location can only be specified when the role is LocationManager.");
        public static readonly Error EmployeeRequiredForEmployeeRole = Error.Validation("OrganizationMember.EmployeeRequiredForEmployeeRole", "An employee must be specified when the role is Employee.");
        public static readonly Error EmployeeNotAllowedForRole = Error.Validation("OrganizationMember.EmployeeNotAllowedForRole", "An employee can only be specified when the role is Employee.");
    }

    public static class Location
    {
        public static readonly Error OrganizationIdEmpty = Error.Validation("Location.OrganizationIdEmpty", "OrganizationId is required.");
        public static readonly Error NameEmpty = Error.Validation("Location.NameEmpty", "Location name cannot be empty.");
        public static readonly Error NameTooLong = Error.Validation("Location.NameTooLong", $"Location name cannot exceed {Entities.Location.MaxNameLength} characters.");
        public static readonly Error InvalidTimeZone = Error.Validation("Location.InvalidTimeZone", "Time zone is not a recognized time zone identifier.");
    }

    public static class Client
    {
        public static readonly Error UserIdEmpty = Error.Validation("Client.UserIdEmpty", "UserId is required.");
        public static readonly Error AlreadyLinkedToDifferentUser = Error.Conflict("Client.AlreadyLinkedToDifferentUser", "This client record is already linked to a different user account.");
    }

    public static class Employee
    {
        public static readonly Error OrganizationIdEmpty = Error.Validation("Employee.OrganizationIdEmpty", "OrganizationId is required.");
        public static readonly Error UserIdEmpty = Error.Validation("Employee.UserIdEmpty", "UserId is required.");
        public static readonly Error FullNameEmpty = Error.Validation("Employee.FullNameEmpty", "Employee full name cannot be empty.");
        public static readonly Error FullNameTooLong = Error.Validation("Employee.FullNameTooLong", $"Employee full name cannot exceed {Entities.Employee.MaxFullNameLength} characters.");
        public static readonly Error InvalidPhotoUrl = Error.Validation("Employee.InvalidPhotoUrl", $"Photo URL must be an absolute http or https URL of at most {Entities.Employee.MaxPhotoUrlLength} characters.");
        public static readonly Error AlreadyLinkedToDifferentUser = Error.Conflict("Employee.AlreadyLinkedToDifferentUser", "This employee record is already linked to a different user account.");
    }

    public static class EmployeeLocationAssignment
    {
        public static readonly Error EmployeeIdEmpty = Error.Validation("EmployeeLocationAssignment.EmployeeIdEmpty", "EmployeeId is required.");
        public static readonly Error LocationIdEmpty = Error.Validation("EmployeeLocationAssignment.LocationIdEmpty", "LocationId is required.");
    }

    public static class Service
    {
        public static readonly Error OrganizationIdEmpty = Error.Validation("Service.OrganizationIdEmpty", "OrganizationId is required.");
        public static readonly Error NameEmpty = Error.Validation("Service.NameEmpty", "Service name cannot be empty.");
        public static readonly Error NameTooLong = Error.Validation("Service.NameTooLong", $"Service name cannot exceed {Entities.Service.MaxNameLength} characters.");
        public static readonly Error DurationNotPositive = Error.Validation("Service.DurationNotPositive", "Duration must be greater than zero.");
        public static readonly Error NegativeBuffer = Error.Validation("Service.NegativeBuffer", "Buffer times cannot be negative.");
        public static readonly Error InvalidColor = Error.Validation("Service.InvalidColor", "Color must be a hex code in the format #RRGGBB.");
    }

    public static class LocationServiceOverride
    {
        public static readonly Error LocationIdEmpty = Error.Validation("LocationServiceOverride.LocationIdEmpty", "LocationId is required.");
        public static readonly Error ServiceIdEmpty = Error.Validation("LocationServiceOverride.ServiceIdEmpty", "ServiceId is required.");
    }

    public static class RecurringSchedule
    {
        public static readonly Error EmployeeLocationAssignmentIdEmpty = Error.Validation("RecurringSchedule.EmployeeLocationAssignmentIdEmpty", "EmployeeLocationAssignmentId is required.");
        public static readonly Error StartNotBeforeEnd = Error.Validation("RecurringSchedule.StartNotBeforeEnd", "Start time must be earlier than end time.");
    }

    public static class ScheduleException
    {
        public static readonly Error EmployeeLocationAssignmentIdEmpty = Error.Validation("ScheduleException.EmployeeLocationAssignmentIdEmpty", "EmployeeLocationAssignmentId is required.");
        public static readonly Error StartNotBeforeEnd = Error.Validation("ScheduleException.StartNotBeforeEnd", "Modified start time must be earlier than modified end time.");
    }

    public static class Booking
    {
        public static readonly Error OrganizationIdEmpty = Error.Validation("Booking.OrganizationIdEmpty", "OrganizationId is required.");
        public static readonly Error LocationIdEmpty = Error.Validation("Booking.LocationIdEmpty", "LocationId is required.");
        public static readonly Error EmployeeIdEmpty = Error.Validation("Booking.EmployeeIdEmpty", "EmployeeId is required.");
        public static readonly Error ServiceIdEmpty = Error.Validation("Booking.ServiceIdEmpty", "ServiceId is required.");
        public static readonly Error ClientIdEmpty = Error.Validation("Booking.ClientIdEmpty", "ClientId is required.");
        public static readonly Error SlotInPast = Error.Validation("Booking.SlotInPast", "The booking's time slot must be in the future.");
        public static readonly Error CancellationReasonTooLong = Error.Validation("Booking.CancellationReasonTooLong", $"Cancellation reason cannot exceed {Entities.Booking.MaxCancellationReasonLength} characters.");
        public static readonly Error CannotConfirm = Error.Conflict("Booking.CannotConfirm", "Only a pending booking can be confirmed.");
        public static readonly Error CannotTransitionToAwaitingReview = Error.Conflict("Booking.CannotTransitionToAwaitingReview", "Only a confirmed booking can transition to awaiting review.");
        public static readonly Error SlotNotYetEnded = Error.Conflict("Booking.SlotNotYetEnded", "The booking's time slot has not ended yet.");
        public static readonly Error CannotComplete = Error.Conflict("Booking.CannotComplete", "Only a booking awaiting review can be marked completed.");
        public static readonly Error CannotMarkNoShow = Error.Conflict("Booking.CannotMarkNoShow", "Only a booking awaiting review can be marked as a no-show.");
        public static readonly Error CannotCancel = Error.Conflict("Booking.CannotCancel", "Only a pending or confirmed booking can be cancelled.");
        public static readonly Error CannotExpire = Error.Conflict("Booking.CannotExpire", "Only a pending booking can expire.");
        public static readonly Error CannotReschedule = Error.Conflict("Booking.CannotReschedule", "Only a pending or confirmed booking can be rescheduled.");
    }

    public static class WaitlistEntry
    {
        public static readonly Error OrganizationIdEmpty = Error.Validation("WaitlistEntry.OrganizationIdEmpty", "OrganizationId is required.");
        public static readonly Error LocationIdEmpty = Error.Validation("WaitlistEntry.LocationIdEmpty", "LocationId is required.");
        public static readonly Error EmployeeIdEmpty = Error.Validation("WaitlistEntry.EmployeeIdEmpty", "EmployeeId is required.");
        public static readonly Error ServiceIdEmpty = Error.Validation("WaitlistEntry.ServiceIdEmpty", "ServiceId is required.");
        public static readonly Error OfferedEmployeeIdEmpty = Error.Validation("WaitlistEntry.OfferedEmployeeIdEmpty", "OfferedEmployeeId is required.");
        public static readonly Error SlotInPast = Error.Validation("WaitlistEntry.SlotInPast", "The time slot must be in the future.");
        public static readonly Error OfferOutsideDesiredWindow = Error.Validation("WaitlistEntry.OfferOutsideDesiredWindow", "The offered slot falls outside the client's desired window.");
        public static readonly Error CannotOffer = Error.Conflict("WaitlistEntry.CannotOffer", "Only a waiting entry can receive an offer.");
        public static readonly Error CannotConvert = Error.Conflict("WaitlistEntry.CannotConvert", "Only an offered entry can be converted into a booking.");
        public static readonly Error CannotExpire = Error.Conflict("WaitlistEntry.CannotExpire", "Only an offered entry can expire.");
        public static readonly Error OfferNotYetExpired = Error.Conflict("WaitlistEntry.OfferNotYetExpired", "The offer's confirmation window has not elapsed yet.");
        public static readonly Error CannotCancel = Error.Conflict("WaitlistEntry.CannotCancel", "Only a waiting or offered entry can be cancelled.");
    }

    public static class Review
    {
        public static readonly Error OrganizationIdEmpty = Error.Validation("Review.OrganizationIdEmpty", "OrganizationId is required.");
        public static readonly Error LocationIdEmpty = Error.Validation("Review.LocationIdEmpty", "LocationId is required.");
        public static readonly Error EmployeeIdEmpty = Error.Validation("Review.EmployeeIdEmpty", "EmployeeId is required.");
        public static readonly Error BookingIdEmpty = Error.Validation("Review.BookingIdEmpty", "BookingId is required.");
        public static readonly Error RatingOutOfRange = Error.Validation("Review.RatingOutOfRange", $"Rating must be between {Entities.Review.MinRating} and {Entities.Review.MaxRating}.");
        public static readonly Error CommentTooLong = Error.Validation("Review.CommentTooLong", $"Comment cannot exceed {Entities.Review.MaxCommentLength} characters.");
    }
}