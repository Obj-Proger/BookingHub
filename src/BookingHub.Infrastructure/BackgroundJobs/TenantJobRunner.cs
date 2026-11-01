using BookingHub.Application.Common.Messaging;
using BookingHub.Infrastructure.Logging;
using BookingHub.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BookingHub.Infrastructure.BackgroundJobs;

/// <summary>
/// Runs a background command once per organization, each in its own DI scope, transaction and
/// tenant scope — so row-level security and the query filters apply to background work exactly as
/// they do to an API request. One organization failing does not stop the others; the job still
/// fails at the end, so the failure is visible in Hangfire and the job is retried.
/// </summary>
internal sealed class TenantJobRunner(IServiceScopeFactory scopeFactory, ILogger<TenantJobRunner> logger)
{
    /// <returns>The sum of the counts the command reported for every organization.</returns>
    public async Task<int> RunForEachOrganizationAsync(Func<Guid, ICommand<int>> createCommand, CancellationToken cancellationToken)
    {
        var organizationIds = await GetOrganizationIdsAsync(cancellationToken);

        var total = 0;
        var failed = new List<Guid>();

        foreach (var organizationId in organizationIds)
        {
            var command = createCommand(organizationId);
            try
            {
                total += await RunForOrganizationAsync(organizationId, command, cancellationToken);
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                Log.BackgroundCommandFailed(logger, exception, command.GetType().Name, organizationId);
                failed.Add(organizationId);
            }
        }

        if (failed.Count > 0)
        {
            throw new InvalidOperationException(
                $"The background command failed for {failed.Count} organization(s): {string.Join(", ", failed)}. See the log for details.");
        }

        return total;
    }

    private async Task<int> RunForOrganizationAsync(Guid organizationId, ICommand<int> command, CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var services = scope.ServiceProvider;

        var dbContext = services.GetRequiredService<ApplicationDbContext>();
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        await services.GetRequiredService<RowLevelSecuritySession>().EnterAsync(organizationId, cancellationToken);

        var result = await services.GetRequiredService<IDispatcher>().Send(command, cancellationToken);
        if (result.IsFailure)
            throw new InvalidOperationException($"{command.GetType().Name} returned [{result.Error.Code}] {result.Error.Message}");

        await transaction.CommitAsync(cancellationToken);
        return result.Value;
    }

    private async Task<List<Guid>> GetOrganizationIdsAsync(CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();

        // Organizations is the tenant root: it has no row-level security, so it can be listed without a scope.
        return await scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().Organizations
            .Select(o => o.Id)
            .ToListAsync(cancellationToken);
    }
}