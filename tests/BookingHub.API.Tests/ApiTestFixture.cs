using BookingHub.Tests.Shared;

namespace BookingHub.API.Tests;

public sealed class ApiTestFixture : IAsyncLifetime
{
    private TestPostgreSql _database = null!;

    public ApiWebApplicationFactory Factory { get; private set; } = null!;

    public async ValueTask InitializeAsync()
    {
        _database = await TestPostgreSql.StartAsync(TestContext.Current.CancellationToken);
        Factory = new ApiWebApplicationFactory(_database.ConnectionString);
    }

    public async ValueTask DisposeAsync()
    {
        await Factory.DisposeAsync();
        await _database.DisposeAsync();
    }
}

[CollectionDefinition(nameof(ApiCollection))]
public sealed class ApiCollection : ICollectionFixture<ApiTestFixture>;