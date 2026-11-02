using System.Diagnostics.CodeAnalysis;
using Npgsql;
using Testcontainers.PostgreSql;

namespace BookingHub.Tests.Shared;

/// <summary>
/// A PostgreSQL database for one test fixture. By default a throwaway container (Testcontainers,
/// which needs a Docker engine whose published ports this machine can reach — as in CI). When the
/// <see cref="ServerVariable"/> environment variable holds a superuser connection string, a fresh
/// database is created on that server instead and dropped afterwards: for a PostgreSQL already
/// running elsewhere, where containers with published ports are not wanted.
/// </summary>
[SuppressMessage("Security", "CA2100", Justification = "DDL takes no parameters; the names are generated here, never input.")]
internal sealed class TestPostgreSql : IAsyncDisposable
{
    public const string ServerVariable = "BOOKINGHUB_TEST_POSTGRES";

    /// <summary>Created by the migrations; see Persistence/Sql/04-application-role.up.sql.</summary>
    private const string ApplicationRole = "bookinghub_app";

    private const string NamePrefix = "bookinghub_test_";

    private readonly PostgreSqlContainer? _container;
    private readonly string? _serverConnectionString;
    private readonly string? _databaseName;
    private readonly List<string> _logins = [];

    private TestPostgreSql(string connectionString, PostgreSqlContainer? container, string? serverConnectionString, string? databaseName)
    {
        ConnectionString = connectionString;
        _container = container;
        _serverConnectionString = serverConnectionString;
        _databaseName = databaseName;
    }

    /// <summary>Connects as a superuser, which bypasses row-level security — for migrating and seeding.</summary>
    public string ConnectionString { get; }

    public static async Task<TestPostgreSql> StartAsync(CancellationToken cancellationToken)
    {
        var server = Environment.GetEnvironmentVariable(ServerVariable);
        if (string.IsNullOrWhiteSpace(server))
        {
            var container = new PostgreSqlBuilder("postgres:18").Build();
            await container.StartAsync(cancellationToken);
            return new TestPostgreSql(container.GetConnectionString(), container, null, null);
        }

        var databaseName = NewName();
        await ExecuteAsync(server, $"CREATE DATABASE \"{databaseName}\"", cancellationToken);

        var connectionString = new NpgsqlConnectionStringBuilder(server) { Database = databaseName }.ConnectionString;
        return new TestPostgreSql(connectionString, null, server, databaseName);
    }

    /// <summary>
    /// A new login that is a member of the application role, so it has exactly the application's
    /// privileges and, being neither a superuser nor the owner, is subject to row-level security.
    /// Call it after migrating: the migrations create the role. A login of its own, with a random
    /// password, leaves the shared role — and any other database on the same server — untouched.
    /// </summary>
    public async Task<string> CreateApplicationLoginAsync(CancellationToken cancellationToken)
    {
        var login = NewName();
        var password = Guid.NewGuid().ToString("N");

        await ExecuteAsync(
            ConnectionString, $"CREATE ROLE \"{login}\" LOGIN PASSWORD '{password}' IN ROLE {ApplicationRole}", cancellationToken);
        _logins.Add(login);

        return new NpgsqlConnectionStringBuilder(ConnectionString) { Username = login, Password = password }.ConnectionString;
    }

    public async ValueTask DisposeAsync()
    {
        if (_container is not null)
        {
            await _container.DisposeAsync();
            return;
        }

        NpgsqlConnection.ClearAllPools();
        await ExecuteAsync(_serverConnectionString!, $"DROP DATABASE IF EXISTS \"{_databaseName}\" WITH (FORCE)", CancellationToken.None);

        foreach (var login in _logins)
            await ExecuteAsync(_serverConnectionString!, $"DROP ROLE IF EXISTS \"{login}\"", CancellationToken.None);
    }

    private static string NewName() => NamePrefix + Guid.NewGuid().ToString("N");

    private static async Task ExecuteAsync(string connectionString, string sql, CancellationToken cancellationToken)
    {
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new NpgsqlCommand(sql, connection);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }
}