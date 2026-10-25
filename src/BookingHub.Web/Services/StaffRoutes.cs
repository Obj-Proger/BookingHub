namespace BookingHub.Web.Services;

internal static class StaffRoutes
{
    public const string Home = "/app";
    public const string NewOrganization = "/app/new";

    public static string Organization(Guid organizationId) => $"{Home}/{organizationId}";

    public static string Locations(Guid organizationId) => $"{Organization(organizationId)}/locations";

    public static string NewLocation(Guid organizationId) => $"{Locations(organizationId)}/new";

    public static string Location(Guid organizationId, Guid locationId) => $"{Locations(organizationId)}/{locationId}";

    /// <summary>Extracts the organization id from a URI shaped like /app/{organizationId}/…</summary>
    public static bool TryGetOrganizationId(string uri, out Guid organizationId)
    {
        organizationId = Guid.Empty;

        var segments = new Uri(uri).AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries);
        return segments is ["app", var id, ..] && Guid.TryParse(id, out organizationId);
    }
}