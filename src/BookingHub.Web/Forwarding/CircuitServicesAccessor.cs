namespace BookingHub.Web.Forwarding;

/// <summary>
/// The current circuit's services, for code that runs in a different DI scope — an HttpClient
/// message handler gets its own. Set while the circuit handles each inbound event (the pattern
/// from the Blazor docs, "Access server-side Blazor services from a different DI scope").
/// Registered as a singleton, so the one async-local value is shared by every circuit's flow.
/// </summary>
internal sealed class CircuitServicesAccessor
{
    private readonly AsyncLocal<IServiceProvider?> _circuitServices = new();

    public IServiceProvider? Services
    {
        get => _circuitServices.Value;
        set => _circuitServices.Value = value;
    }
}