using BookingHub.Infrastructure.IntegrationTests.TestDoubles;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Testcontainers.PostgreSql;

namespace BookingHub.Infrastructure.IntegrationTests;

public sealed class PostgreSqlFixture : IAsyncLifetime
{
    private const string AppRolePassword = "test-only-password";
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgres:18")
    .Build();

    public string ConnectionString => _container.GetConnectionString();

    public string AppRoleConnectionString =>
        new NpgsqlConnectionStringBuilder(ConnectionString) { Username = "bookinghub_app", Password = AppRolePassword }.ConnectionString;

    /// <summary>Owner-role context — bypasses RLS, used for seeding and for tests unrelated to it (unchanged behavior from before this commit).</summary>
    public ApplicationDbContext CreateDbContext() =>
        new(new DbContextOptionsBuilder<ApplicationDbContext>().UseNpgsql(ConnectionString).Options, new FixedCurrentTenant(null));

    /// <summary>Least-privilege role context — actually subject to RLS, unlike the owner above.</summary>
    public ApplicationDbContext CreateAppRoleDbContext() =>
        new(new DbContextOptionsBuilder<ApplicationDbContext>().UseNpgsql(AppRoleConnectionString).Options, new FixedCurrentTenant(null));

    public async ValueTask InitializeAsync()
    {
        await _container.StartAsync();

        await using var dbContext = CreateDbContext();
        await dbContext.Database.MigrateAsync();
        await dbContext.Database.ExecuteSqlRawAsync($"ALTER ROLE bookinghub_app WITH PASSWORD '{AppRolePassword}';");
    }

    public async ValueTask DisposeAsync() => await _container.DisposeAsync();
}

[CollectionDefinition(nameof(DatabaseCollection))]
public sealed class DatabaseCollection : ICollectionFixture<PostgreSqlFixture>;