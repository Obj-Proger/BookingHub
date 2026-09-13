using BookingHub.API.Common;
using BookingHub.Application.Features.Employees.Commands.AssignEmployeeToLocation;
using BookingHub.Application.Features.Employees.Commands.CreateDayOffScheduleException;
using BookingHub.Application.Features.Employees.Commands.CreateEmployee;
using BookingHub.Application.Features.Employees.Commands.CreateModifiedHoursScheduleException;
using BookingHub.Application.Features.Employees.Commands.CreateRecurringSchedule;
using BookingHub.Application.Features.Employees.Commands.DeactivateEmployeeAssignment;
using BookingHub.Application.Features.Employees.Commands.RemoveRecurringSchedule;
using BookingHub.Application.Features.Employees.Commands.RemoveScheduleException;
using BookingHub.Application.Features.Employees.Commands.RenameEmployee;
using BookingHub.Application.Features.Employees.Commands.RescheduleRecurringSchedule;
using BookingHub.Application.Features.Employees.Commands.SetEmployeeBookable;
using BookingHub.Application.Features.Employees.Commands.UpdateEmployeePhoto;
using BookingHub.Application.Features.Employees.Queries.GetEmployee;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookingHub.API.Controllers.Employees;

[Authorize]
[Route("api/v1/organizations/{organizationId:guid}")]
public sealed class EmployeesController(IDispatcher dispatcher) : ApiControllerBase
{
    // Employee profile (organization-wide)

    [HttpPost("employees")]
    public async Task<IActionResult> Create(Guid organizationId, CreateEmployeeRequest request, CancellationToken cancellationToken)
    {
        var result = await dispatcher.Send(new CreateEmployeeCommand(organizationId, request.FullName), cancellationToken);
        return HandleResult(result);
    }

    [HttpGet("employees/{employeeId:guid}")]
    public async Task<IActionResult> Get(Guid organizationId, Guid employeeId, CancellationToken cancellationToken)
    {
        var result = await dispatcher.Send(new GetEmployeeQuery(organizationId, employeeId), cancellationToken);
        return HandleResult(result);
    }

    [HttpPut("employees/{employeeId:guid}/name")]
    public async Task<IActionResult> Rename(Guid organizationId, Guid employeeId, RenameEmployeeRequest request, CancellationToken cancellationToken)
    {
        var result = await dispatcher.Send(new RenameEmployeeCommand(organizationId, employeeId, request.NewFullName), cancellationToken);
        return HandleResult(result);
    }

    [HttpPut("employees/{employeeId:guid}/bookable")]
    public async Task<IActionResult> SetBookable(
        Guid organizationId, Guid employeeId, SetEmployeeBookableRequest request, CancellationToken cancellationToken)
    {
        var result = await dispatcher.Send(new SetEmployeeBookableCommand(organizationId, employeeId, request.IsBookable), cancellationToken);
        return HandleResult(result);
    }

    [HttpPut("employees/{employeeId:guid}/photo")]
    public async Task<IActionResult> UpdatePhoto(
        Guid organizationId, Guid employeeId, UpdateEmployeePhotoRequest request, CancellationToken cancellationToken)
    {
        var result = await dispatcher.Send(new UpdateEmployeePhotoCommand(organizationId, employeeId, request.PhotoUrl), cancellationToken);
        return HandleResult(result);
    }

    // Assignment to a location (location-scoped)

    [HttpPost("locations/{locationId:guid}/employees")]
    public async Task<IActionResult> AssignToLocation(
        Guid organizationId, Guid locationId, AssignEmployeeToLocationRequest request, CancellationToken cancellationToken)
    {
        var result = await dispatcher.Send(new AssignEmployeeToLocationCommand(organizationId, locationId, request.EmployeeId), cancellationToken);
        return HandleResult(result);
    }

    [HttpDelete("locations/{locationId:guid}/employees/{assignmentId:guid}")]
    public async Task<IActionResult> DeactivateAssignment(
        Guid organizationId, Guid locationId, Guid assignmentId, CancellationToken cancellationToken)
    {
        var result = await dispatcher.Send(new DeactivateEmployeeAssignmentCommand(organizationId, locationId, assignmentId), cancellationToken);
        return HandleResult(result);
    }

    // Recurring schedule (location-scoped; Create needs the owning assignment, the rest only need their own id)

    [HttpPost("locations/{locationId:guid}/assignments/{assignmentId:guid}/schedule/recurring")]
    public async Task<IActionResult> CreateRecurringSchedule(
        Guid organizationId, Guid locationId, Guid assignmentId, CreateRecurringScheduleRequest request, CancellationToken cancellationToken)
    {
        var result = await dispatcher.Send(
            new CreateRecurringScheduleCommand(organizationId, locationId, assignmentId, request.DayOfWeek, request.StartTime, request.EndTime),
            cancellationToken);
        return HandleResult(result);
    }

    [HttpPut("locations/{locationId:guid}/schedule/recurring/{recurringScheduleId:guid}")]
    public async Task<IActionResult> RescheduleRecurringSchedule(
        Guid organizationId, Guid locationId, Guid recurringScheduleId, RescheduleRecurringScheduleRequest request, CancellationToken cancellationToken)
    {
        var result = await dispatcher.Send(
            new RescheduleRecurringScheduleCommand(organizationId, locationId, recurringScheduleId, request.NewStartTime, request.NewEndTime),
            cancellationToken);
        return HandleResult(result);
    }

    [HttpDelete("locations/{locationId:guid}/schedule/recurring/{recurringScheduleId:guid}")]
    public async Task<IActionResult> RemoveRecurringSchedule(
        Guid organizationId, Guid locationId, Guid recurringScheduleId, CancellationToken cancellationToken)
    {
        var result = await dispatcher.Send(new RemoveRecurringScheduleCommand(organizationId, locationId, recurringScheduleId), cancellationToken);
        return HandleResult(result);
    }

    // Schedule exceptions (same nesting rule as recurring schedule)

    [HttpPost("locations/{locationId:guid}/assignments/{assignmentId:guid}/schedule/exceptions/day-off")]
    public async Task<IActionResult> CreateDayOffException(
        Guid organizationId, Guid locationId, Guid assignmentId, CreateDayOffScheduleExceptionRequest request, CancellationToken cancellationToken)
    {
        var result = await dispatcher.Send(
            new CreateDayOffScheduleExceptionCommand(organizationId, locationId, assignmentId, request.Date), cancellationToken);
        return HandleResult(result);
    }

    [HttpPost("locations/{locationId:guid}/assignments/{assignmentId:guid}/schedule/exceptions/modified-hours")]
    public async Task<IActionResult> CreateModifiedHoursException(
        Guid organizationId, Guid locationId, Guid assignmentId, CreateModifiedHoursScheduleExceptionRequest request, CancellationToken cancellationToken)
    {
        var result = await dispatcher.Send(
            new CreateModifiedHoursScheduleExceptionCommand(
                organizationId, locationId, assignmentId, request.Date, request.ModifiedStartTime, request.ModifiedEndTime),
            cancellationToken);
        return HandleResult(result);
    }

    [HttpDelete("locations/{locationId:guid}/schedule/exceptions/{scheduleExceptionId:guid}")]
    public async Task<IActionResult> RemoveScheduleException(
        Guid organizationId, Guid locationId, Guid scheduleExceptionId, CancellationToken cancellationToken)
    {
        var result = await dispatcher.Send(new RemoveScheduleExceptionCommand(organizationId, locationId, scheduleExceptionId), cancellationToken);
        return HandleResult(result);
    }
}