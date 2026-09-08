using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;

namespace BookingHub.Infrastructure.IntegrationTests;

/// <summary>
/// One PostgreSQL container, one schema migration, shared across every test in the collection —
/// starting a fresh container per test would work but cost seconds per test, not milliseconds;
/// shared across a whole run is the standard Testcontainers pattern for this reason.
/// </summary>

public sealed class PostgreSqlFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder()
        .WithImage("postgres:18")
        .Build();

    public string ConnectionString => _container.GetConnectionString();

    public ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>().UseNpgsql(ConnectionString).Options;
        return new ApplicationDbContext(options);
    }

    public async ValueTask InitializeAsync()
    {
        await _container.StartAsync();

        await using var dbContext = CreateDbContext();
        await dbContext.Database.MigrateAsync();
    }

    public async ValueTask DisposeAsync() => await _container.DisposeAsync();
}

[CollectionDefinition(nameof(DatabaseCollection))]
public sealed class DatabaseCollection : ICollectionFixture<PostgreSqlFixture>;