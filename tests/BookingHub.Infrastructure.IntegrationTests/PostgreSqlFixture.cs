using BookingHub.Infrastructure.IntegrationTests.TestDoubles;
using BookingHub.Tests.Shared;
using Microsoft.EntityFrameworkCore;

namespace BookingHub.Infrastructure.IntegrationTests;

public sealed class PostgreSqlFixture : IAsyncLifetime
{
    private TestPostgreSql _database = null!;

    public string ConnectionString => _database.ConnectionString;

    /// <summary>A login in the application role — subject to row-level security, unlike <see cref="ConnectionString"/>.</summary>
    public string AppRoleConnectionString { get; private set; } = null!;

    /// <summary>Superuser context — bypasses RLS, used for seeding and for tests unrelated to it.</summary>
    public ApplicationDbContext CreateDbContext() =>
        new(new DbContextOptionsBuilder<ApplicationDbContext>().UseNpgsql(ConnectionString).Options, new FixedCurrentTenant(null));

    /// <summary>Least-privilege context — actually subject to RLS, unlike the superuser above.</summary>
    public ApplicationDbContext CreateAppRoleDbContext() =>
        new(new DbContextOptionsBuilder<ApplicationDbContext>().UseNpgsql(AppRoleConnectionString).Options, new FixedCurrentTenant(null));

    public async ValueTask InitializeAsync()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        _database = await TestPostgreSql.StartAsync(cancellationToken);

        await using var dbContext = CreateDbContext();
        await dbContext.Database.MigrateAsync(cancellationToken);

        AppRoleConnectionString = await _database.CreateApplicationLoginAsync(cancellationToken);
    }

    public async ValueTask DisposeAsync() => await _database.DisposeAsync();
}

[CollectionDefinition(nameof(DatabaseCollection))]
public sealed class DatabaseCollection : ICollectionFixture<PostgreSqlFixture>;