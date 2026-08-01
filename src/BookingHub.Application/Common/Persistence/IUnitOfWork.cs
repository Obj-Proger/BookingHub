namespace BookingHub.Application.Common.Persistence;

/// <summary>Commits everything changed through repositories in the current request as one transaction.</summary>
public interface IUnitOfWork
{
    Task SaveChangesAsync(CancellationToken cancellationToken);
}