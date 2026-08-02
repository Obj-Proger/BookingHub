using BookingHub.Application.Common.Messaging;
using BookingHub.Application.Common.Security;

namespace BookingHub.Application.Features.Employees.Commands.UpdateEmployeePhoto;

public sealed record UpdateEmployeePhotoCommand(Guid OrganizationId, Guid EmployeeId, string? PhotoUrl)
    : ICommand, IRequireOrganizationManagement;