namespace BookingHub.Application.Common;

/// <summary>
/// Errors that require infrastructure knowledge (existence in the database, uniqueness)
/// and therefore cannot be expressed inside Domain — see <c>DomainErrors</c> for
/// errors that a Domain factory/method can determine on its own.
/// </summary>
public static class ApplicationErrors
{
    public static class Organization
    {
        public static readonly Error SlugAlreadyTaken = new("Organization.SlugAlreadyTaken", "This slug is already in use by another organization.", ErrorType.Conflict);
        public static readonly Error NotFound = new("Organization.NotFound", "Organization not found.", ErrorType.NotFound);
    }
}