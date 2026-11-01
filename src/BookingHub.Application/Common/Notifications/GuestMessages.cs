using System.Globalization;
using BookingHub.Application.Common.Persistence;
using BookingHub.Domain.Entities;
using BookingHub.Domain.ValueObjects;

namespace BookingHub.Application.Common.Notifications;

/// <summary>
/// Words each message, builds its link into the Web app and shows times in the location's own time
/// zone. Organization slugs and time zones are looked up once per scope, so a background command
/// that messages many guests of one organization does not repeat the lookups.
/// </summary>
internal sealed class GuestMessages(
    IGuestNotifier notifier,
    IWebLinkBuilder linkBuilder,
    IOrganizationRepository organizationRepository,
    ILocationRepository locationRepository)
    : IGuestMessages
{
    private const string TimeFormat = "ddd d MMM yyyy, HH:mm";

    private readonly Dictionary<Guid, string> _slugs = [];
    private readonly Dictionary<Guid, TimeZoneInfo> _timeZones = [];

    public async Task BookingRequestedAsync(Booking firstBooking, int visitCount, CancellationToken cancellationToken)
    {
        var link = await BuildBookingLinkAsync(firstBooking, "confirm", firstBooking.ConfirmationToken, cancellationToken);
        var start = await FormatLocalTimeAsync(firstBooking.OrganizationId, firstBooking.LocationId, firstBooking.TimeSlot.StartUtc, cancellationToken);

        if (visitCount > 1)
        {
            Notify(firstBooking, "Confirm your bookings",
                $"Confirm your {visitCount}-visit series starting {start}: {link}");
        }
        else
        {
            Notify(firstBooking, "Confirm your booking", $"Confirm your booking for {start}: {link}");
        }
    }

    public async Task BookingConfirmedAsync(Booking booking, CancellationToken cancellationToken)
    {
        var link = await BuildBookingLinkAsync(booking, "manage", booking.CancellationToken, cancellationToken);
        var start = await FormatLocalTimeAsync(booking.OrganizationId, booking.LocationId, booking.TimeSlot.StartUtc, cancellationToken);

        Notify(booking, "Booking confirmed",
            $"Your booking for {start} is confirmed. Manage it (cancel or reschedule) here: {link}");
    }

    public async Task BookingRescheduledAsync(Booking booking, CancellationToken cancellationToken)
    {
        var link = await BuildBookingLinkAsync(booking, "manage", booking.CancellationToken, cancellationToken);
        var start = await FormatLocalTimeAsync(booking.OrganizationId, booking.LocationId, booking.TimeSlot.StartUtc, cancellationToken);

        Notify(booking, "Booking rescheduled", $"Your booking has been moved to {start}. Manage it here: {link}");
    }

    public async Task BookingCancelledAsync(Booking booking, CancellationToken cancellationToken)
    {
        var start = await FormatLocalTimeAsync(booking.OrganizationId, booking.LocationId, booking.TimeSlot.StartUtc, cancellationToken);

        Notify(booking, "Booking cancelled", $"Your booking for {start} has been cancelled.");
    }

    public async Task ReviewRequestedAsync(Booking booking, CancellationToken cancellationToken)
    {
        var link = await BuildBookingLinkAsync(booking, "review", booking.CancellationToken, cancellationToken);

        Notify(booking, "How was your visit?", $"Thanks for visiting! Mind leaving a quick review? {link}");
    }

    public async Task WaitlistSlotOfferedAsync(WaitlistEntry entry, CancellationToken cancellationToken)
    {
        if (entry.OfferedSlot is null || entry.OfferExpiresAtUtc is null)
            throw new InvalidOperationException($"Waitlist entry {entry.Id} holds no offer.");

        var slug = await GetSlugAsync(entry.OrganizationId, cancellationToken);
        var link = linkBuilder.Build($"/book/{slug}/waitlist/{entry.Id}?token={entry.ManagementToken.Value}");
        var start = await FormatLocalTimeAsync(entry.OrganizationId, entry.LocationId, entry.OfferedSlot.StartUtc, cancellationToken);
        var expires = await FormatLocalTimeAsync(entry.OrganizationId, entry.LocationId, entry.OfferExpiresAtUtc.Value, cancellationToken);

        notifier.Notify(entry.OrganizationId, entry.ClientContact, "A slot opened up",
            $"A slot opened up: {start}. Confirm by {expires}: {link}");
    }

    private void Notify(Booking booking, string subject, string body) =>
        notifier.Notify(booking.OrganizationId, booking.ClientContact, subject, body);

    /// <param name="page">The guest page under /book/{slug}/: confirm, manage or review.</param>
    private async Task<string> BuildBookingLinkAsync(
        Booking booking, string page, SecurityToken token, CancellationToken cancellationToken)
    {
        var slug = await GetSlugAsync(booking.OrganizationId, cancellationToken);
        return linkBuilder.Build($"/book/{slug}/{page}/{booking.Id}?token={token.Value}");
    }

    private async Task<string> GetSlugAsync(Guid organizationId, CancellationToken cancellationToken)
    {
        if (!_slugs.TryGetValue(organizationId, out var slug))
        {
            slug = await organizationRepository.GetSlugAsync(organizationId, cancellationToken)
                ?? throw new InvalidOperationException($"Organization {organizationId} does not exist.");
            _slugs[organizationId] = slug;
        }

        return slug;
    }

    private async Task<string> FormatLocalTimeAsync(
        Guid organizationId, Guid locationId, DateTime utc, CancellationToken cancellationToken)
    {
        if (!_timeZones.TryGetValue(locationId, out var timeZone))
        {
            var location = await locationRepository.GetByIdAsync(organizationId, locationId, cancellationToken)
                ?? throw new InvalidOperationException($"Location {locationId} does not exist.");
            timeZone = TimeZoneInfo.FindSystemTimeZoneById(location.TimeZone);
            _timeZones[locationId] = timeZone;
        }

        return TimeZoneInfo.ConvertTimeFromUtc(utc, timeZone).ToString(TimeFormat, CultureInfo.InvariantCulture);
    }
}