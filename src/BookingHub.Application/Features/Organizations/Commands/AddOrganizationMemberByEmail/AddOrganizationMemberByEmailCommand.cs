using BookingHub.Application.Common.Messaging;
using BookingHub.Application.Common.Security;
using BookingHub.Application.Features.Organizations.DTOs;
using BookingHub.Domain.Enums;

namespace BookingHub.Application.Features.Organizations.Commands.AddOrganizationMemberByEmail;

public sealed record AddOrganizationMemberByEmailCommand(
    Guid OrganizationId, string? Email, OrganizationRole Role, Guid? LocationId, Guid? EmployeeId)
    : ICommand<OrganizationMemberCreatedResponse>, IRequireOrganizationManagement;