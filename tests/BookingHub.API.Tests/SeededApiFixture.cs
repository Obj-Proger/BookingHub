using System.Globalization;
using BookingHub.Infrastructure.Persistence.Seeding;
using BookingHub.Tests.Shared;

namespace BookingHub.API.Tests;

/// <summary>
/// An API over a database of its own filled by the development seed — the same demo data the app is
/// tried with by hand. The seed needs an empty database, so this cannot share <see cref="ApiTestFixture"/>.
/// </summary>
public sealed class SeededApiFixture : IAsyncLifetime
{
    private TestPostgreSql _database = null!;

    public ApiWebApplicationFactory Factory { get; private set; } = null!;

    /// <summary>What the seed printed: the accounts and the guest links with their tokens.</summary>
    public string SeedOutput { get; private set; } = null!;

    public async ValueTask InitializeAsync()
    {
        var cancellationToken = TestContext.Current.CancellationToken;

        _database = await TestPostgreSql.StartAsync(cancellationToken);
        Factory = new ApiWebApplicationFactory(_database.ConnectionString);
        await Factory.MigrateDatabaseAsync();

        await using var output = new StringWriter(CultureInfo.InvariantCulture);
        if (!await DevelopmentSeeder.SeedAsync(Factory.Services, output, cancellationToken))
            throw new InvalidOperationException($"The seed did not run: {output}");

        SeedOutput = output.ToString();
    }

    public async ValueTask DisposeAsync()
    {
        await Factory.DisposeAsync();
        await _database.DisposeAsync();
    }
}
