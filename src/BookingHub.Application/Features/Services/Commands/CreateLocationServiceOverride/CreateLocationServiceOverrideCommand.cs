using BookingHub.Application.Common.Messaging;
using BookingHub.Application.Common.Security;
using BookingHub.Application.Features.Services.DTOs;

namespace BookingHub.Application.Features.Services.Commands.CreateLocationServiceOverride;

public sealed record CreateLocationServiceOverrideCommand(
    Guid OrganizationId, Guid LocationId, Guid ServiceId, decimal OverridePriceAmount, string? OverridePriceCurrency)
    : ICommand<LocationServiceOverrideCreatedResponse>, IRequireLocationManagement;