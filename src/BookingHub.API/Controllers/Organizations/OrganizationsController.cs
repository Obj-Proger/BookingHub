using BookingHub.API.Common;
using BookingHub.Application.Features.Organizations.Commands.AddOrganizationMember;
using BookingHub.Application.Features.Organizations.Commands.ChangeOrganizationMemberRole;
using BookingHub.Application.Features.Organizations.Commands.CreateOrganization;
using BookingHub.Application.Features.Organizations.Commands.RemoveOrganizationMember;
using BookingHub.Application.Features.Organizations.Commands.RenameOrganization;
using BookingHub.Application.Features.Organizations.Commands.SetOrganizationAdministratorFinancialAccess;
using BookingHub.Application.Features.Organizations.Commands.UpdateOrganizationAutoCompleteWindow;
using BookingHub.Application.Features.Organizations.Commands.UpdateOrganizationCancellationDeadline;
using BookingHub.Application.Features.Organizations.Commands.UpdateOrganizationPendingConfirmationWindow;
using BookingHub.Application.Features.Organizations.Commands.UpdateOrganizationWaitlistOfferWindow;
using BookingHub.Application.Features.Organizations.Queries.GetOrganization;
using BookingHub.Application.Features.Organizations.Queries.GetOrganizationMembers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookingHub.API.Controllers.Organizations;

[Authorize]
[Route("api/v1/organizations")]
public sealed class OrganizationsController(IDispatcher dispatcher) : ApiControllerBase
{
    // Organization itself

    [HttpPost]
    public async Task<IActionResult> Create(CreateOrganizationRequest request, CancellationToken cancellationToken)
    {
        var result = await dispatcher.Send(new CreateOrganizationCommand(request.Name, request.Slug), cancellationToken);
        return HandleResult(result);
    }

    [HttpGet("{organizationId:guid}")]
    public async Task<IActionResult> Get(Guid organizationId, CancellationToken cancellationToken)
    {
        var result = await dispatcher.Send(new GetOrganizationQuery(organizationId), cancellationToken);
        return HandleResult(result);
    }

    [HttpPut("{organizationId:guid}/name")]
    public async Task<IActionResult> Rename(Guid organizationId, RenameOrganizationRequest request, CancellationToken cancellationToken)
    {
        var result = await dispatcher.Send(new RenameOrganizationCommand(organizationId, request.NewName), cancellationToken);
        return HandleResult(result);
    }

    // Members

    [HttpGet("{organizationId:guid}/members")]
    public async Task<IActionResult> GetMembers(Guid organizationId, CancellationToken cancellationToken)
    {
        var result = await dispatcher.Send(new GetOrganizationMembersQuery(organizationId), cancellationToken);
        return HandleResult(result);
    }

    [HttpPost("{organizationId:guid}/members")]
    public async Task<IActionResult> AddMember(Guid organizationId, AddOrganizationMemberRequest request, CancellationToken cancellationToken)
    {
        var result = await dispatcher.Send(
            new AddOrganizationMemberCommand(organizationId, request.UserId, request.Role, request.LocationId, request.EmployeeId),
            cancellationToken);
        return HandleResult(result);
    }

    [HttpPatch("{organizationId:guid}/members/{organizationMemberId:guid}")]
    public async Task<IActionResult> ChangeMemberRole(
        Guid organizationId, Guid organizationMemberId, ChangeOrganizationMemberRoleRequest request, CancellationToken cancellationToken)
    {
        var result = await dispatcher.Send(
            new ChangeOrganizationMemberRoleCommand(organizationId, organizationMemberId, request.NewRole, request.LocationId, request.EmployeeId),
            cancellationToken);
        return HandleResult(result);
    }

    [HttpDelete("{organizationId:guid}/members/{organizationMemberId:guid}")]
    public async Task<IActionResult> RemoveMember(Guid organizationId, Guid organizationMemberId, CancellationToken cancellationToken)
    {
        var result = await dispatcher.Send(new RemoveOrganizationMemberCommand(organizationId, organizationMemberId), cancellationToken);
        return HandleResult(result);
    }

    // Settings

    [HttpPut("{organizationId:guid}/settings/cancellation-deadline")]
    public async Task<IActionResult> UpdateCancellationDeadline(
        Guid organizationId, UpdateCancellationDeadlineRequest request, CancellationToken cancellationToken)
    {
        var result = await dispatcher.Send(new UpdateOrganizationCancellationDeadlineCommand(organizationId, request.Deadline), cancellationToken);
        return HandleResult(result);
    }

    [HttpPut("{organizationId:guid}/settings/pending-confirmation-window")]
    public async Task<IActionResult> UpdatePendingConfirmationWindow(
        Guid organizationId, UpdateSchedulingWindowRequest request, CancellationToken cancellationToken)
    {
        var result = await dispatcher.Send(new UpdateOrganizationPendingConfirmationWindowCommand(organizationId, request.Window), cancellationToken);
        return HandleResult(result);
    }

    [HttpPut("{organizationId:guid}/settings/auto-complete-window")]
    public async Task<IActionResult> UpdateAutoCompleteWindow(
        Guid organizationId, UpdateSchedulingWindowRequest request, CancellationToken cancellationToken)
    {
        var result = await dispatcher.Send(new UpdateOrganizationAutoCompleteWindowCommand(organizationId, request.Window), cancellationToken);
        return HandleResult(result);
    }

    [HttpPut("{organizationId:guid}/settings/waitlist-offer-window")]
    public async Task<IActionResult> UpdateWaitlistOfferWindow(
        Guid organizationId, UpdateSchedulingWindowRequest request, CancellationToken cancellationToken)
    {
        var result = await dispatcher.Send(new UpdateOrganizationWaitlistOfferWindowCommand(organizationId, request.Window), cancellationToken);
        return HandleResult(result);
    }

    [HttpPut("{organizationId:guid}/settings/administrator-financial-access")]
    public async Task<IActionResult> SetAdministratorFinancialAccess(
        Guid organizationId, SetAdministratorFinancialAccessRequest request, CancellationToken cancellationToken)
    {
        var result = await dispatcher.Send(
            new SetOrganizationAdministratorFinancialAccessCommand(organizationId, request.Enabled), cancellationToken);
        return HandleResult(result);
    }
}