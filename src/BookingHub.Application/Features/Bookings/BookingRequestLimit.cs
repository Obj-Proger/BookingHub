using BookingHub.Application.Common.Persistence;
using BookingHub.Domain.Entities;

namespace BookingHub.Application.Features.Bookings;

/// <summary>
/// Caps how many bookings one phone number can request per hour in an organization. Every request
/// texts a confirmation link to the number it names, so without a cap anyone could flood a
/// stranger's phone — and the organization's SMS bill — from the public booking page, whatever
/// the per-IP limits. A recurring series counts as one request.
/// </summary>
internal static class BookingRequestLimit
{
    public const int MaxRequestsPerWindow = 5;

    public static readonly TimeSpan Window = TimeSpan.FromHours(1);

    /// <param name="client">The client with the requested phone number, or null when the number is new.</param>
    public static async Task<bool> IsReachedAsync(
        IBookingRepository bookingRepository, Guid organizationId, Client? client, DateTime utcNow,
        CancellationToken cancellationToken)
    {
        if (client is null)
            return false;

        var requests = await bookingRepository.CountPublicRequestsSinceAsync(
            organizationId, client.Id, utcNow - Window, cancellationToken);

        return requests >= MaxRequestsPerWindow;
    }
}