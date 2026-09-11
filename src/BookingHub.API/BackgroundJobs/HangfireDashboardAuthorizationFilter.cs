using Hangfire.Dashboard;

namespace BookingHub.API.BackgroundJobs;

/// <summary>
/// Fails closed outside Development — there is no "platform administrator" concept anywhere in
/// this project's authorization model (every role is scoped to one organization), so there is
/// currently no principled way to say who should see this. Locking it down entirely is the
/// safe default until that question is deliberately answered, not an oversight.
/// </summary>
internal sealed class HangfireDashboardAuthorizationFilter(IWebHostEnvironment environment) : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context) => environment.IsDevelopment();
}