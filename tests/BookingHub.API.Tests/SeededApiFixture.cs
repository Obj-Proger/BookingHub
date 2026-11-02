using System.Globalization;
using BookingHub.API.Tests.TestDoubles;
using BookingHub.Infrastructure.Persistence.Seeding;
using BookingHub.Tests.Shared;

namespace BookingHub.API.Tests;

/// <summary>
/// An API over a database of its own filled by the development seed — the same demo data the app is
/// tried with by hand. The seed needs an empty database, so this cannot share <see cref="ApiTestFixture"/>;
/// as a class fixture, every test class that uses it gets a fresh copy.
/// </summary>
public sealed class SeededApiFixture : IAsyncLifetime
{
    private TestPostgreSql _database = null!;

    public ApiWebApplicationFactory Factory { get; private set; } = null!;

    public DemoData Demo { get; private set; } = null!;

    public GuestInbox Inbox => Factory.Inbox;

    public async ValueTask InitializeAsync()
    {
        var cancellationToken = TestContext.Current.CancellationToken;

        _database = await TestPostgreSql.StartAsync(cancellationToken);
        Factory = new ApiWebApplicationFactory(_database.ConnectionString);

        await using var output = new StringWriter(CultureInfo.InvariantCulture);
        if (!await DevelopmentSeeder.SeedAsync(Factory.Services, output, cancellationToken))
            throw new InvalidOperationException($"The seed did not run: {output}");

        Demo = await DemoData.LoadAsync(Factory.Services, output.ToString(), cancellationToken);
    }

    public async ValueTask DisposeAsync()
    {
        await Factory.DisposeAsync();
        await _database.DisposeAsync();
    }
}
