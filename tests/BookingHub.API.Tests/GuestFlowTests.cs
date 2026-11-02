using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using BookingHub.API.Controllers.Bookings;
using BookingHub.API.Controllers.Reviews;
using BookingHub.API.Controllers.Waitlist;
using BookingHub.Application.Features.Bookings.DTOs;
using BookingHub.Application.Features.Reviews.DTOs;
using BookingHub.Domain.Enums;
using BookingHub.Infrastructure.Persistence.Seeding;

namespace BookingHub.API.Tests;

/// <summary>
/// What a guest does, end to end through the API on the demo data, with the links they are texted:
/// book, confirm, reschedule and cancel; take a time freed for the waitlist; review a visit. Every
/// write goes through the real database, the outbox and its event handlers.
/// </summary>
public class GuestFlowTests(SeededApiFixture fixture) : IClassFixture<SeededApiFixture>
{
    /// <summary>The per-phone cap in BookingRequestLimit (internal to the Application project).</summary>
    private const int BookingRequestsPerHour = 5;

    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    private static readonly string PublicApi = $"api/v1/public/{DevelopmentSeeder.OrganizationSlug}";

    private DemoData Demo => fixture.Demo;

    [Fact]
    public async Task Booking_ConfirmedRescheduledAndCancelled_GuestIsTextedAtEveryStep()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        using var client = fixture.Factory.CreateClient();
        const string Phone = "+79001110001";

        var slots = await FindFreeSlotsAsync(client, Demo.RiversideId, Demo.ClaraId, count: 2, cancellationToken);
        var (first, later) = (slots[0], slots[^1]);

        var booking = await PostForAsync<BookingCreatedResponse>(client, $"{PublicApi}/bookings",
            new CreateBookingRequest(Demo.RiversideId, Demo.ClaraId, Demo.HaircutId, first.StartUtc, Phone, "Test Guest", null),
            cancellationToken);
        booking.Status.Should().Be(BookingStatus.Pending);

        var confirmLink = await fixture.Inbox.WaitForLinkAsync(Phone, "confirm", booking.BookingId, cancellationToken);
        await PostAsync(client, $"{PublicApi}/bookings/{booking.BookingId}/confirm",
            new ConfirmBookingRequest(confirmLink.Token), cancellationToken);

        var manageLink = await fixture.Inbox.WaitForLinkAsync(Phone, "manage", booking.BookingId, cancellationToken);
        (await GetBookingAsync(client, manageLink, cancellationToken)).Status.Should().Be(BookingStatus.Confirmed);

        await PostAsync(client, $"{PublicApi}/bookings/{booking.BookingId}/reschedule",
            new RescheduleBookingRequest(manageLink.Token, later.StartUtc), cancellationToken);
        await fixture.Inbox.WaitForAsync(Phone, body => body.Contains("has been moved to", StringComparison.Ordinal), cancellationToken);
        (await GetBookingAsync(client, manageLink, cancellationToken)).StartUtc.Should().Be(later.StartUtc);

        await PostAsync(client, $"{PublicApi}/bookings/{booking.BookingId}/cancel",
            new CancelBookingRequest(manageLink.Token, "Plans changed"), cancellationToken);
        await fixture.Inbox.WaitForAsync(Phone, body => body.Contains("has been cancelled", StringComparison.Ordinal), cancellationToken);
        (await GetBookingAsync(client, manageLink, cancellationToken)).Status.Should().Be(BookingStatus.Cancelled);
    }

    [Fact]
    public async Task CancelledBooking_TimeIsOfferedToTheWaitlist_WhichTakesIt()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        using var client = fixture.Factory.CreateClient();

        var ivansBooking = await GetBookingAsync(client, Demo.IvanBooking, cancellationToken);
        await PostAsync(client, $"{PublicApi}/bookings/{Demo.IvanBooking.Id}/cancel",
            new CancelBookingRequest(Demo.IvanBooking.Token, null), cancellationToken);

        var offerLink = await fixture.Inbox.WaitForLinkAsync(DemoData.PavelPhone, "waitlist", null, cancellationToken);
        var pavelsBooking = await PostForAsync<BookingCreatedResponse>(client, $"{PublicApi}/waitlist/{offerLink.Id}/confirm",
            new ConfirmWaitlistOfferRequest(offerLink.Token), cancellationToken);

        pavelsBooking.Status.Should().Be(BookingStatus.Confirmed);
        pavelsBooking.StartUtc.Should().Be(ivansBooking.StartUtc);
    }

    [Fact]
    public async Task CompletedVisit_GuestIsAskedForAReview_WhichAppearsOnTheEmployeesPage()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        using var client = fixture.Factory.CreateClient();
        const string Comment = "Quick and tidy, thank you.";

        var ownerToken = await TestHelpers.SignInAsync(client, DemoData.OwnerEmail, DevelopmentSeeder.Password, cancellationToken);
        using (var complete = new HttpRequestMessage(HttpMethod.Post,
            $"api/v1/organizations/{Demo.OrganizationId}/locations/{Demo.CentralId}/employees/{Demo.AnnaId}/bookings/{Demo.OlegReview.Id}/complete")
            .WithBearerToken(ownerToken))
        {
            using var response = await client.SendAsync(complete, cancellationToken);
            await ShouldSucceedAsync(response, cancellationToken);
        }

        var reviewLink = await fixture.Inbox.WaitForLinkAsync(DemoData.OlegPhone, "review", Demo.OlegReview.Id, cancellationToken);
        await PostAsync(client, $"{PublicApi}/bookings/{reviewLink.Id}/review",
            new SubmitReviewRequest(reviewLink.Token, 5, Comment), cancellationToken);

        var reviews = await client.GetFromJsonAsync<List<ReviewResponse>>(
            $"{PublicApi}/employees/{Demo.AnnaId}/reviews", Json, cancellationToken);
        reviews.Should().ContainSingle(review => review.Comment == Comment && review.Rating == 5);
    }

    [Fact]
    public async Task OnePhone_MoreBookingRequestsThanTheHourlyCap_TheExtraOneIsRefused()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        using var client = fixture.Factory.CreateClient();
        const string Phone = "+79001110002";

        var slots = await FindFreeSlotsAsync(client, Demo.CentralId, Demo.BorisId, BookingRequestsPerHour + 1, cancellationToken);

        foreach (var slot in slots.Take(BookingRequestsPerHour))
        {
            await PostForAsync<BookingCreatedResponse>(client, $"{PublicApi}/bookings",
                new CreateBookingRequest(Demo.CentralId, Demo.BorisId, Demo.HaircutId, slot.StartUtc, Phone, null, null),
                cancellationToken);
        }

        using var refused = await client.PostAsJsonAsync($"{PublicApi}/bookings",
            new CreateBookingRequest(Demo.CentralId, Demo.BorisId, Demo.HaircutId, slots[BookingRequestsPerHour].StartUtc, Phone, null, null),
            cancellationToken);

        refused.StatusCode.Should().Be(HttpStatusCode.TooManyRequests);
    }

    /// <summary>
    /// Haircut times that do not overlap one another on the first day that has <paramref name="count"/> of
    /// them, at least two days ahead: a guest cannot change a booking within the 24-hour cancellation deadline.
    /// </summary>
    private async Task<List<AvailableSlotResponse>> FindFreeSlotsAsync(
        HttpClient client, Guid locationId, Guid employeeId, int count, CancellationToken cancellationToken)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        for (var daysAhead = 2; daysAhead <= 21; daysAhead++)
        {
            var date = today.AddDays(daysAhead).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            var available = await client.GetFromJsonAsync<List<AvailableSlotResponse>>(
                $"{PublicApi}/locations/{locationId}/employees/{employeeId}/services/{Demo.HaircutId}/availability?date={date}",
                Json, cancellationToken);

            var separate = new List<AvailableSlotResponse>();
            foreach (var slot in available!)
            {
                if (separate.Count == 0 || slot.StartUtc >= separate[^1].EndUtc)
                    separate.Add(slot);
            }

            if (separate.Count >= count)
                return separate;
        }

        throw new InvalidOperationException($"No day in the next three weeks has {count} separate free times.");
    }

    private static async Task<PublicBookingDetailsResponse> GetBookingAsync(HttpClient client, GuestLink link, CancellationToken cancellationToken) =>
        (await client.GetFromJsonAsync<PublicBookingDetailsResponse>(
            $"{PublicApi}/bookings/{link.Id}?token={Uri.EscapeDataString(link.Token)}", Json, cancellationToken))!;

    private static async Task PostAsync<TRequest>(HttpClient client, string url, TRequest body, CancellationToken cancellationToken)
    {
        using var response = await client.PostAsJsonAsync(url, body, cancellationToken);
        await ShouldSucceedAsync(response, cancellationToken);
    }

    private static async Task<TResponse> PostForAsync<TResponse>(HttpClient client, string url, object body, CancellationToken cancellationToken)
    {
        using var response = await client.PostAsJsonAsync(url, body, cancellationToken);
        await ShouldSucceedAsync(response, cancellationToken);

        return (await response.Content.ReadFromJsonAsync<TResponse>(Json, cancellationToken))!;
    }

    private static async Task ShouldSucceedAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            Assert.Fail($"{response.RequestMessage?.Method} {response.RequestMessage?.RequestUri} answered {(int)response.StatusCode}: {body}");
        }
    }
}
