using System.Linq.Expressions;
using BookingHub.Application.Features.Employees.DTOs;
using BookingHub.Domain.Entities;

namespace BookingHub.Application.Features.Employees;

internal static class EmployeeProjections
{
    public static readonly Expression<Func<Employee, EmployeeResponse>> ToResponse =
        e => new EmployeeResponse(e.Id, e.FullName, e.PhotoUrl, e.IsBookable);
}