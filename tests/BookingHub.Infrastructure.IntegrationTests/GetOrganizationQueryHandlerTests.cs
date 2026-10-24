using BookingHub.Application.Features.Organizations.Queries.GetOrganization;

namespace BookingHub.Infrastructure.IntegrationTests;

[Collection(nameof(DatabaseCollection))]
public class GetOrganizationQueryHandlerTests(PostgreSqlFixture fixture)
{
    [Fact]
    public async Task Handle_AdministratorFinancialAccessEnabled_ReturnsTheFlag()
    {
        var organization = Organization.Create("Acme", $"acme-{Guid.CreateVersion7()}").Value;
        organization.SetAdministratorFinancialAccess(true);

        await using (var seedContext = fixture.CreateDbContext())
        {
            seedContext.Organizations.Add(organization);
            await seedContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        await using var dbContext = fixture.CreateDbContext();
        var handler = new GetOrganizationQueryHandler(dbContext);

        var result = await handler.Handle(new GetOrganizationQuery(organization.Id), TestContext.Current.CancellationToken);

        result.Value.CanAdministratorsViewFinancials.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_OrganizationDoesNotExist_ReturnsNotFound()
    {
        await using var dbContext = fixture.CreateDbContext();
        var handler = new GetOrganizationQueryHandler(dbContext);

        var result = await handler.Handle(new GetOrganizationQuery(Guid.CreateVersion7()), TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
    }
}