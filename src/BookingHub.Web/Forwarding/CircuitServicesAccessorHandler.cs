using Microsoft.AspNetCore.Components.Server.Circuits;

namespace BookingHub.Web.Forwarding;

/// <summary>Publishes the circuit's services through <see cref="CircuitServicesAccessor"/> while it handles an inbound event.</summary>
internal sealed class CircuitServicesAccessorHandler(IServiceProvider services, CircuitServicesAccessor accessor) : CircuitHandler
{
    public override Func<CircuitInboundActivityContext, Task> CreateInboundActivityHandler(
        Func<CircuitInboundActivityContext, Task> next) =>
        async context =>
        {
            accessor.Services = services;
            await next(context);
            accessor.Services = null;
        };
}