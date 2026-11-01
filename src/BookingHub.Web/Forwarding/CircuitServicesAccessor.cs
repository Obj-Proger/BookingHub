namespace BookingHub.Web.Forwarding;

/// <summary>
/// The current circuit's services, for code that runs in a different DI scope — an HttpClient
/// message handler gets its own. Set while the circuit handles each inbound event (the pattern
/// from the Blazor docs, "Access server-side Blazor services from a different DI scope").
/// </summary>
internal sealed class CircuitServicesAccessor
{
    private static readonly AsyncLocal<IServiceProvider?> CircuitServices = new();

    public IServiceProvider? Services
    {
        get => CircuitServices.Value;
        set => CircuitServices.Value = value;
    }
}