using BookingHub.Application.Common;
using BookingHub.Application.Common.Messaging;
using BookingHub.Application.Common.Persistence;
using BookingHub.Application.Common.Security;
using BookingHub.Application.Features.Organizations.DTOs;
using BookingHub.Domain.Entities;
using BookingHub.Domain.Enums;

namespace BookingHub.Application.Features.Organizations.Commands.CreateOrganization;

internal sealed class CreateOrganizationCommandHandler(
    IOrganizationRepository organizationRepository,
    IOrganizationMemberRepository organizationMemberRepository,
    ICurrentUser currentUser,
    IUnitOfWork unitOfWork)
    : ICommandHandler<CreateOrganizationCommand, OrganizationCreatedResponse>
{
    public async Task<Result<OrganizationCreatedResponse>> Handle(CreateOrganizationCommand command, CancellationToken cancellationToken)
    {
        if (await organizationRepository.SlugExistsAsync(command.Slug ?? string.Empty, cancellationToken))
            return Result.Failure<OrganizationCreatedResponse>(ApplicationErrors.Organization.SlugAlreadyTaken);

        var organizationResult = Organization.Create(command.Name, command.Slug);
        if (organizationResult.IsFailure)
            return Result.Failure<OrganizationCreatedResponse>(organizationResult.Error);

        var organization = organizationResult.Value;

        var memberResult = OrganizationMember.Create(organization.Id, currentUser.UserId, OrganizationRole.Owner);
        if (memberResult.IsFailure)
            return Result.Failure<OrganizationCreatedResponse>(memberResult.Error);

        organizationRepository.Add(organization);
        organizationMemberRepository.Add(memberResult.Value);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new OrganizationCreatedResponse(organization.Id, organization.Name, organization.Slug);
    }
}