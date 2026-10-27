namespace BookingHub.Web.Services;

internal static class StaffRoutes
{
    public const string Home = "/app";
    public const string NewOrganization = "/app/new";

    public static string Organization(Guid organizationId) => $"{Home}/{organizationId}";

    public static string Locations(Guid organizationId) => $"{Organization(organizationId)}/locations";

    public static string NewLocation(Guid organizationId) => $"{Locations(organizationId)}/new";

    public static string Location(Guid organizationId, Guid locationId) => $"{Locations(organizationId)}/{locationId}";

    public static string Services(Guid organizationId) => $"{Organization(organizationId)}/services";

    public static string NewService(Guid organizationId) => $"{Services(organizationId)}/new";

    public static string Service(Guid organizationId, Guid serviceId) => $"{Services(organizationId)}/{serviceId}";

    public static string Employees(Guid organizationId) => $"{Organization(organizationId)}/employees";

    public static string NewEmployee(Guid organizationId) => $"{Employees(organizationId)}/new";

    public static string Employee(Guid organizationId, Guid employeeId) => $"{Employees(organizationId)}/{employeeId}";

    public static string Schedule(Guid organizationId, Guid locationId, Guid assignmentId) =>
    $"{Location(organizationId, locationId)}/schedule/{assignmentId}";

    public static string Clients(Guid organizationId) => $"{Organization(organizationId)}/clients";

    public static string Client(Guid organizationId, Guid clientId) => $"{Clients(organizationId)}/{clientId}";

    /// <summary>The client search page, opened with this phone number already searched for.</summary>
    public static string ClientSearch(Guid organizationId, string phone) =>
        $"{Clients(organizationId)}?phone={Uri.EscapeDataString(phone)}";

    /// <summary>Extracts the organization id from a URI shaped like /app/{organizationId}/…</summary>
    public static bool TryGetOrganizationId(string uri, out Guid organizationId)
    {
        organizationId = Guid.Empty;

        var segments = new Uri(uri).AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries);
        return segments is ["app", var id, ..] && Guid.TryParse(id, out organizationId);
    }
}