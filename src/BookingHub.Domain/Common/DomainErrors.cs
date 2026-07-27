namespace BookingHub.Domain.Common;

/// <summary>
/// Central catalog of domain-specific <see cref="Error"/> instances, grouped by concept.
/// </summary>
public static class DomainErrors
{
    public static class Email
    {
        public static readonly Error Empty = new("Email.Empty", "Email address cannot be empty.", ErrorType.Validation);
        public static readonly Error TooLong = new("Email.TooLong", "Email address exceeds the maximum allowed length.", ErrorType.Validation);
        public static readonly Error InvalidFormat = new("Email.InvalidFormat", "Email address is not in a valid format.", ErrorType.Validation);
    }

    public static class PhoneNumber
    {
        public static readonly Error Empty = new("PhoneNumber.Empty", "Phone number cannot be empty.", ErrorType.Validation);
        public static readonly Error InvalidFormat = new("PhoneNumber.InvalidFormat", "Phone number must be in international E.164 format, e.g. +14155552671.", ErrorType.Validation);
    }

    public static class Money
    {
        public static readonly Error NegativeAmount = new("Money.NegativeAmount", "Amount cannot be negative.", ErrorType.Validation);
        public static readonly Error InvalidCurrency = new("Money.InvalidCurrency", "Currency must be a 3-letter ISO 4217 code.", ErrorType.Validation);
    }

    public static class Address
    {
        public static readonly Error Empty = new("Address.Empty", "Address cannot be empty.", ErrorType.Validation);
        public static readonly Error TooLong = new("Address.TooLong", "Address exceeds the maximum allowed length.", ErrorType.Validation);
    }

    public static class TimeSlot
    {
        public static readonly Error NotUtc = new("TimeSlot.NotUtc", "Start and end must be expressed in UTC.", ErrorType.Validation);
        public static readonly Error StartNotBeforeEnd = new("TimeSlot.StartNotBeforeEnd", "Start must be earlier than end.", ErrorType.Validation);
    }

    public static class DailyHours
    {
        public static readonly Error OpenNotBeforeClose = new("DailyHours.OpenNotBeforeClose", "Opening time must be earlier than closing time.", ErrorType.Validation);
    }

    public static class WeeklyHours
    {
        public static readonly Error MustCoverAllDays = new("WeeklyHours.MustCoverAllDays", "Weekly hours must define exactly one entry for each day of the week.", ErrorType.Validation);
    }

    public static class Organization
    {
        public static readonly Error NameEmpty = new("Organization.NameEmpty", "Organization name cannot be empty.", ErrorType.Validation);
        public static readonly Error NameTooLong = new("Organization.NameTooLong", "Organization name exceeds the maximum allowed length.", ErrorType.Validation);
        public static readonly Error SlugEmpty = new("Organization.SlugEmpty", "Organization slug cannot be empty.", ErrorType.Validation);
        public static readonly Error SlugInvalidFormat = new("Organization.SlugInvalidFormat", "Slug must contain only lowercase letters, digits, and hyphens, and cannot start or end with a hyphen.", ErrorType.Validation);
    }

    public static class Location
    {
        public static readonly Error NameEmpty = new("Location.NameEmpty", "Location name cannot be empty.", ErrorType.Validation);
        public static readonly Error NameTooLong = new("Location.NameTooLong", "Location name exceeds the maximum allowed length.", ErrorType.Validation);
        public static readonly Error InvalidTimeZone = new("Location.InvalidTimeZone", "Time zone is not a recognized system time zone identifier.", ErrorType.Validation);
    }
}