namespace BookingHub.Mobile.Services;

public interface IAppSessionContext
{
    Guid? OrganizationId { get; }
    string? OrganizationName { get; }
    Guid? LocationId { get; }
    string? LocationName { get; }
    Guid? EmployeeId { get; }
    bool IsComplete { get; }

    void Save(Guid organizationId, string organizationName, Guid locationId, string locationName, Guid employeeId);
    void Clear();
}