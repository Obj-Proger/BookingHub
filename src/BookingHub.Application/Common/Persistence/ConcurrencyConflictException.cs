namespace BookingHub.Application.Common.Persistence;

/// <summary>
/// Thrown by <see cref="IUnitOfWork.SaveChangesAsync"/> when a database-level exclusivity
/// constraint (the double-booking EXCLUDE constraint) is violated by a concurrent write.
/// Infrastructure translates the underlying PostgreSQL error into this type so the handful
/// of handlers that care can catch it without depending on the database provider.
/// </summary>
public sealed class ConcurrencyConflictException : Exception;