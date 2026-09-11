using BookingHub.Application.Common.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace BookingHub.Infrastructure.Persistence;

/// <summary>
/// Used only by EF Core design-time tooling (`dotnet ef migrations add`/`database update`) —
/// Infrastructure is a class library with no host of its own, and API's Program.cs doesn't yet
/// call AddInfrastructure (not written yet), so neither can supply DbContextOptions the normal
/// way. The connection string below is a design-time placeholder only: `migrations add` never
/// actually connects, it only needs to know the provider (Npgsql) to generate the right SQL.
/// </summary>
public sealed class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql("Host=localhost;Database=bookinghub_design;Username=postgres;Password=postgres");

        return new ApplicationDbContext(optionsBuilder.Options, new DesignTimeCurrentTenant());
    }

    private sealed class DesignTimeCurrentTenant : ICurrentTenant
    {
        public Guid? OrganizationId => null;
    }
}