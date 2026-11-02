using BookingHub.Application.Common.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace BookingHub.Infrastructure.Persistence;

/// <summary>
/// Used only by EF Core design-time tooling (`dotnet ef migrations add`/`database update`), so the
/// tools need no startup project: Infrastructure is a class library without a host of its own.
/// The connection string is a placeholder — `migrations add` never connects, it only needs the
/// provider — and `database update` is always given the real one with --connection (see README).
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