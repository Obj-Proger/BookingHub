using Microsoft.AspNetCore.Identity;

namespace BookingHub.Infrastructure.Identity;

/// <summary>
/// The ASP.NET Core Identity user. Deliberately minimal — this project's authorization model
/// (<see cref="Domain.Entities.OrganizationMember"/>) lives entirely in Domain/Application;
/// Identity's own role system is not used at all (see <see cref="Persistence.ApplicationDbContext"/>,
/// which derives from <c>IdentityUserContext</c>, not <c>IdentityDbContext</c>).
/// </summary>
public sealed class ApplicationUser : IdentityUser<Guid>;