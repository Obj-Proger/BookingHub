using BookingHub.Application.Common.Messaging;
using BookingHub.Application.Common.Security;

namespace BookingHub.Application.Features.Organizations.Commands.SetOrganizationAdministratorFinancialAccess;

/// <summary>Owner-only — checked in the handler, not via a pipeline marker, mirroring how
/// OrganizationMember role changes touching Owner are guarded (Commit — organization members).</summary>
public sealed record SetOrganizationAdministratorFinancialAccessCommand(Guid OrganizationId, bool Enabled)
    : ICommand, IRequireOrganizationManagement;