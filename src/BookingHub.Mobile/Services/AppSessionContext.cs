namespace BookingHub.Mobile.Services;

/// <summary>
/// The resolved working context — not a secret, so Preferences (plain, unencrypted app storage),
/// not SecureStorage (reserved for the tokens themselves). Persists across app restarts.
/// </summary>
/// <remarks>
/// Members intentionally stay instance-level (not static, despite touching only the ambient
/// Preferences API) — same reasoning as SecureTokenStore: this is a stateful service behind
/// its own interface for DI/testability, not a static utility.
/// </remarks>
internal sealed class AppSessionContext : IAppSessionContext
{
    private const string OrganizationIdKey = "session_organization_id";
    private const string OrganizationNameKey = "session_organization_name";
    private const string LocationIdKey = "session_location_id";
    private const string LocationNameKey = "session_location_name";
    private const string EmployeeIdKey = "session_employee_id";

    public Guid? OrganizationId => GetGuid(OrganizationIdKey);
    public string? OrganizationName => Preferences.Default.Get(OrganizationNameKey, (string?)null);
    public Guid? LocationId => GetGuid(LocationIdKey);
    public string? LocationName => Preferences.Default.Get(LocationNameKey, (string?)null);
    public Guid? EmployeeId => GetGuid(EmployeeIdKey);

    public bool IsComplete => OrganizationId is not null && LocationId is not null && EmployeeId is not null;

    public void Save(Guid organizationId, string organizationName, Guid locationId, string locationName, Guid employeeId)
    {
        Preferences.Default.Set(OrganizationIdKey, organizationId.ToString());
        Preferences.Default.Set(OrganizationNameKey, organizationName);
        Preferences.Default.Set(LocationIdKey, locationId.ToString());
        Preferences.Default.Set(LocationNameKey, locationName);
        Preferences.Default.Set(EmployeeIdKey, employeeId.ToString());
    }

    public void Clear() => Preferences.Default.Clear();

    private static Guid? GetGuid(string key)
    {
        var raw = Preferences.Default.Get(key, (string?)null);
        return Guid.TryParse(raw, out var value) ? value : null;
    }
}