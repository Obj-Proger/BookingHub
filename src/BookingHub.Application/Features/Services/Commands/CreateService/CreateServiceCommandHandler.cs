using BookingHub.Application.Common.Messaging;
using BookingHub.Application.Common.Persistence;
using BookingHub.Application.Features.Services.DTOs;
using BookingHub.Domain.Entities;
using BookingHub.Domain.ValueObjects;

namespace BookingHub.Application.Features.Services.Commands.CreateService;

internal sealed class CreateServiceCommandHandler(IServiceRepository serviceRepository, IUnitOfWork unitOfWork)
    : ICommandHandler<CreateServiceCommand, ServiceCreatedResponse>
{
    public async Task<Result<ServiceCreatedResponse>> Handle(CreateServiceCommand command, CancellationToken cancellationToken)
    {
        var priceResult = Money.Create(command.BasePriceAmount, command.BasePriceCurrency);
        if (priceResult.IsFailure)
            return Result.Failure<ServiceCreatedResponse>(priceResult.Error);

        var serviceResult = Service.Create(
            command.OrganizationId, command.Name, command.Duration, priceResult.Value,
            command.BufferBefore, command.BufferAfter, command.Color);
        if (serviceResult.IsFailure)
            return Result.Failure<ServiceCreatedResponse>(serviceResult.Error);

        serviceRepository.Add(serviceResult.Value);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new ServiceCreatedResponse(serviceResult.Value.Id, serviceResult.Value.Name);
    }
}