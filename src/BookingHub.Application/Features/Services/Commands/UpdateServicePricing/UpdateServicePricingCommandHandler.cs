using BookingHub.Application.Common;
using BookingHub.Application.Common.Messaging;
using BookingHub.Application.Common.Persistence;
using BookingHub.Domain.ValueObjects;

namespace BookingHub.Application.Features.Services.Commands.UpdateServicePricing;

internal sealed class UpdateServicePricingCommandHandler(IServiceRepository serviceRepository, IUnitOfWork unitOfWork)
    : ICommandHandler<UpdateServicePricingCommand>
{
    public async Task<Result> Handle(UpdateServicePricingCommand command, CancellationToken cancellationToken)
    {
        var service = await serviceRepository.GetByIdAsync(command.OrganizationId, command.ServiceId, cancellationToken);
        if (service is null)
            return Result.Failure(ApplicationErrors.Service.NotFound);

        var priceResult = Money.Create(command.NewAmount, command.NewCurrency);
        if (priceResult.IsFailure)
            return Result.Failure(priceResult.Error);

        service.UpdatePricing(priceResult.Value);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}