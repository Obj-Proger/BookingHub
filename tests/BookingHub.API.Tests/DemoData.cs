using BookingHub.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BookingHub.API.Tests;

/// <summary>The rows of the seeded demo organization that tests address, and the guest links the seed printed.</summary>
public sealed class DemoData
{
    public const string OwnerEmail = "owner@demo.test";
    public const string EmployeeEmail = "anna@demo.test";

    public const string IvanPhone = "+79001000001";
    public const string OlegPhone = "+79001000003";

    /// <summary>On the waitlist for Anna's day of Ivan's upcoming haircut.</summary>
    public const string PavelPhone = "+79001000005";

    public required Guid OrganizationId { get; init; }
    public required Guid CentralId { get; init; }
    public required Guid RiversideId { get; init; }
    public required Guid AnnaId { get; init; }
    public required Guid BorisId { get; init; }
    public required Guid ClaraId { get; init; }
    public required Guid AnnaAtCentralId { get; init; }
    public required Guid HaircutId { get; init; }
    public required Guid IvanId { get; init; }

    /// <summary>Ivan's confirmed haircut with Anna; cancelling it offers the time to Pavel.</summary>
    public required GuestLink IvanBooking { get; init; }

    /// <summary>Oleg's visit to Anna yesterday, awaiting review; the link works once staff mark it completed.</summary>
    public required GuestLink OlegReview { get; init; }

    public static async Task<DemoData> LoadAsync(IServiceProvider services, string seedOutput, CancellationToken cancellationToken)
    {
        await using var scope = services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var locations = await dbContext.Locations.IgnoreQueryFilters().ToDictionaryAsync(l => l.Name, l => l.Id, cancellationToken);
        var employees = await dbContext.Employees.IgnoreQueryFilters().ToDictionaryAsync(e => e.FullName, e => e.Id, cancellationToken);
        var annaId = employees["Anna Smirnova"];
        var centralId = locations["Central"];

        return new DemoData
        {
            OrganizationId = await dbContext.Organizations.IgnoreQueryFilters().Select(o => o.Id).SingleAsync(cancellationToken),
            CentralId = centralId,
            RiversideId = locations["Riverside"],
            AnnaId = annaId,
            BorisId = employees["Boris Volkov"],
            ClaraId = employees["Clara Novak"],
            AnnaAtCentralId = await dbContext.EmployeeLocationAssignments.IgnoreQueryFilters()
                .Where(a => a.EmployeeId == annaId && a.LocationId == centralId).Select(a => a.Id).SingleAsync(cancellationToken),
            HaircutId = await dbContext.Services.IgnoreQueryFilters()
                .Where(s => s.Name == "Haircut").Select(s => s.Id).SingleAsync(cancellationToken),
            IvanId = await dbContext.Clients.IgnoreQueryFilters()
                .Where(c => c.Name == "Ivan Petrov").Select(c => c.Id).SingleAsync(cancellationToken),
            IvanBooking = GuestLink.Find(seedOutput, "manage")
                ?? throw new InvalidOperationException("The seed printed no manage link."),
            OlegReview = GuestLink.Find(seedOutput, "review")
                ?? throw new InvalidOperationException("The seed printed no review link.")
        };
    }
}
